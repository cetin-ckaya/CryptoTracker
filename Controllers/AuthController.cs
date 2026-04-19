using Microsoft.AspNetCore.Mvc;
using CryptoTracker.DTOs;
using CryptoTracker.Services;

namespace CryptoTracker.Controllers;

// AuthController → sadece isteği alır, işi AuthService'e devreder
// İş mantığı burada değil, Service katmanında
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // IAuthService → interface üzerinden bağlanıyoruz
    // AuthService'in içini bilmemize gerek yok
    private readonly IAuthService _authService;

    // Constructor → IAuthService dependency injection ile otomatik gelir
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register — kayıt isteğini Service'e iletir
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // [FromBody] → isteğin body'sinden JSON olarak RegisterDto'yu al
        // Service'e gönder, sonucu al
        var (success, message) = await _authService.RegisterAsync(dto);

        // Kayıt başarısızsa 400 Bad Request döndür
        if (!success)
            return BadRequest(new { message });

        // Başarılıysa 200 OK döndür
        return Ok(new { message });
    }

    // POST api/auth/login — giriş isteğini Service'e iletir
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Service'ten token al — başarısızsa null gelir
        var token = await _authService.LoginAsync(dto);

        // Token null ise kullanıcı adı veya şifre yanlış
        if (token == null)
            return Unauthorized(new { message = "Email veya şifre hatalı" });

        // Token'ı döndür — frontend bunu saklayacak ve her istekte gönderecek
        return Ok(new { token });
    }
}