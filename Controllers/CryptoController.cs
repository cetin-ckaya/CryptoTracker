using CryptoTracker.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    { 
       // API'nin çalışıp çalışmadığını kontrol etmek için basit bir endpoint
        return Ok(new {message = "CryptoTracker API Çalışıyor"});
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var user = new UserDto
        {
          Id = 1,
          Username = "Çetin",
          Email = "cetin@example.com",
          CreatedAt = DateTime.UtcNow  
        };

        return Ok(user);
    }
}