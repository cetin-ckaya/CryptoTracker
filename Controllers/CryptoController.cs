using AutoMapper;
using CryptoTracker.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CryptoTracker.Models;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{
    // UserManager → giriş yapmış kullanıcının bilgilerini veritabanından çekmek için
    private readonly UserManager<User> _usermanager;

    // IMapper → AutoMapper'ın dönüşüm servisi
    // User → UserDto gibi dönüşümleri bu servis yapıyor
    private readonly IMapper _mapper;

    public CryptoController(UserManager<User> userManager, IMapper mapper)
    {
        _usermanager = userManager;
        _mapper = mapper;
    }

    // GET api/crypto/status — token gerekmez, herkes erişebilir
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new {message = "CryptoTracker API çalışıyor", version = "1.0"});
    }

    // GET api/crypto/me — giriş yapmış kullanıcının bilgilerini döndürür
    // [Authorize] → sadece geçerli JWT token ile erişilebilir
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        // Token içinden kullanıcı ID sini al
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //ID yoksa token geçersiz kıl
        if(userId == null) return Unauthorized();

        //Kullanıcıyı veritabanından ID ile getir
        var user = await _usermanager.FindByIdAsync(userId);

        //kullanıcı bulunmazsa 404 döndür
        if(user == null) return NotFound();

        // User → UserDto dönüşümünü AutoMapper yapıyor
        // Elle her alanı yazmak yerine _mapper.Map kullanıyoruz
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(userDto);
    }

}