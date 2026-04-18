using System.Security.Claims;
using CryptoTracker.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTracker.Controllers;


// Bu controller'daki tüm endpoint'ler api/crypto altında çalışır
[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{

    // GET api/crypto/status — API'nin çalışıp çalışmadığını kontrol eder
    // Token gerekmez, herkes erişebilir
    [HttpGet("status")]
    public IActionResult GetStatus()
    { 
       // API'nin çalışıp çalışmadığını kontrol etmek için basit bir endpoint
        return Ok(new {message = "CryptoTracker API Çalışıyor"});
    }

    // GET api/crypto/me — giriş yapmış kullanıcının bilgilerini döndürür
    // [Authorize] → bu endpoint'e sadece geçerli JWT token ile erişilebilir
    // Token olmadan istek atılırsa otomatik olarak 401 Unauthorized döner
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        // User → ControllerBase'den gelen, giriş yapmış kullanıcının bilgilerini taşır
        // Claims → JWT token içine gömdüğümüz bilgiler (Id, Username, Email)
        // FindFirst → token içindeki belirli bir claim'i bul
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // ?. → eğer FindFirst null dönerse hata vermez, direkt null döner
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        // UserDto döndürüyoruz — PasswordHash gibi hassas alanlar yok
        return Ok(new UserDto
        {
            // Id string çünkü Identity GUID kullanıyor — "abc123-..." formatında
            Id = 0,
            Username = username ?? "",
            Email = email ?? "",
            CreatedAt = DateTime.UtcNow
        });
    }
}