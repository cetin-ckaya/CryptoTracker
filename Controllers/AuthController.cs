using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoTracker.Models;
using CryptoTracker.DTOs;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // UserManager → Identity'nin kullanıcı işlemleri için hazır servisi
    // Kullanıcı oluşturma, şifre kontrolü gibi işlemleri yapar
    private readonly UserManager<User> _userManager;

    // IConfiguration → appsettings.json'daki değerleri okumak için
    private readonly IConfiguration _configuration;

    // Constructor → bu servisler otomatik olarak inject edilir (dependency injection)
    public AuthController(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // POST api/auth/register — yeni kullanıcı kaydı
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Yeni kullanıcı nesnesi oluştur
        var user = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Identity şifreyi otomatik hash'ler ve kullanıcıyı veritabanına kaydeder
        var result = await _userManager.CreateAsync(user, dto.Password);

        // Kayıt başarısız olduysa hataları döndür
        // Örnek: "Bu email zaten kullanılıyor"
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Kayıt başarılı" });
    }

    // POST api/auth/login — giriş yap ve JWT token al
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Email ile kullanıcıyı veritabanında ara
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Kullanıcı bulunamadıysa veya şifre yanlışsa aynı hatayı ver
        // "Email bulunamadı" veya "Şifre yanlış" gibi ayrı mesajlar güvenlik açığı oluşturur
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Email veya şifre hatalı" });

        // JWT token oluştur
        var token = GenerateJwtToken(user);

        return Ok(new { token });
    }

    // JWT token oluşturan yardımcı metod
    private string GenerateJwtToken(User user)
    {
        // appsettings.json'dan JWT ayarlarını oku
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claim → token içine gömmek istediğimiz bilgiler
        // Frontend bu bilgilere token'ı decode ederek erişebilir
        var claims = new[]
        {
            // Kullanıcının ID'si — işlem eklerken hangi kullanıcı olduğunu bileceğiz
            new Claim(ClaimTypes.NameIdentifier, user.Id),

            // Kullanıcı adı
            new Claim(ClaimTypes.Name, user.UserName!),

            // Email
            new Claim(ClaimTypes.Email, user.Email!)
        };

        // Token oluştur — 7 gün geçerli
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        // Token'ı string'e çevir ve döndür
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}