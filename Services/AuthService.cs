using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoTracker.DTOs;
using CryptoTracker.Models;

namespace CryptoTracker.Services;

// AuthService → kayıt ve giriş iş mantığını taşır
// Daha önce bu kodlar AuthController içindeydi
// Şimdi Controller sadece isteği alıp Service'e iletecek
public class AuthService : IAuthService
{
    // UserManager → Identity'nin kullanıcı işlemleri servisi
    private readonly UserManager<User> _userManager;

    // IConfiguration → appsettings.json'daki JWT ayarlarını okumak için
    private readonly IConfiguration _configuration;

    // Constructor → bu servisler dependency injection ile otomatik gelir
    public AuthService(UserManager<User> userManager,IConfiguration configuration)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    // Kayıt işlemi — yeni kullanıcı oluşturur
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
    {
        //Yeni USer nesnesi oluşturur
        var user = new User
        {
          UserName = dto.Username,
          Email = dto.Email,
          CreatedAt = DateTime.UtcNow  
        };

        // Identity şifreyi otomatik hash'ler ve veritabanına kaydeder
        var result = await _userManager.CreateAsync(user, dto.Password);

        // Kayıt başarısız olduysa hataları birleştirip döndür
        // Örnek: "Bu email zaten kullanılıyor"
        if (!result.Succeeded)
        {
            // result.Errors → Identity'nin hata listesi
            // Select → her hatanın sadece açıklama metnini al
            // string.Join → hataları virgülle birleştir
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            return(false,errors);
        }
        return(true,"Kayıt Başarılı");
    }

    // Giriş işlemi — kullanıcıyı doğrular ve JWT token üretir
    public async Task<string?> LoginAsync(LoginDto dto)
    {
        // Email ile kullanıcıyı veritabanında ara
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Kullanıcı yoksa veya şifre yanlışsa null döner
        // Ayrı hata mesajı vermiyoruz — güvenlik açığı oluşturur
        if(user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return null;

        // Kullanıcı doğrulandı, JWT token üret
        return GenerateJwtToken(user);
    }

    // JWT token oluşturan yardımcı metod
    private string GenerateJwtToken(User user)
    {
         // appsettings.json'dan gizli anahtarı oku ve byte dizisine çevir
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        // İmzalama bilgileri — HmacSha256 algoritması ile imzalıyoruz
        // Bu sayede token'ın içeriği değiştirilemez
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claim → token içine gömmek istediğimiz kullanıcı bilgileri
        // Frontend bu bilgilere token'ı decode ederek erişebilir
        var claims = new[]
        {
            // Kullanıcının veritabanı ID'si — işlem eklerken kimin eklediğini bileceğiz
            new Claim(ClaimTypes.NameIdentifier, user.Id),

            // Kullanıcı adı — "Merhaba Çetin" gibi gösterimlerde kullanacağız
            new Claim(ClaimTypes.Name, user.UserName!),

            // Email — profil sayfasında göstereceğiz
            new Claim(ClaimTypes.Email, user.Email!)

        };

        // Token nesnesini oluştur
        var token = new JwtSecurityToken(
            // Kim oluşturdu — appsettings.json'daki "CryptoTracker"
            issuer : _configuration["Jwt:Issuer"],

            // Kime ait — appsettings.json'daki "CryptoTrackerUsers"
            audience : _configuration["Jwt:Audience"],

            // Token içindeki bilgiler
            claims: claims,

            // Token'ın geçerlilik süresi — 7 gün sonra geçersiz olur
            expires : DateTime.UtcNow.AddDays(7),

            //Tokenı İmzala
            signingCredentials: creds
        );

        // Token nesnesini string'e çevir — "eyJhbGci..." formatında
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}