using System.Security.Claims;
using CryptoTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTracker.Controllers;

// PortfolioController → kullanıcının portföy özetini döndürür
// Tüm endpoint'ler token gerektirir
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    // IPortfolioService → portföy hesaplama için
    private readonly IPortfolioService _portfolioservice;

    public PortfolioController(IPortfolioService portfolioservice)
    {
        _portfolioservice = portfolioservice;
    }

    // Token içinden kullanıcı ID'sini alan yardımcı metod
    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // GET api/portfolio — tüm coinlerin portföy özetini getir
    [HttpGet]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        //Service den portfoy hesapla
        var portfolio = await _portfolioservice.GetPortfoliosAsync(userId);

        return Ok(portfolio);
    }

    // GET api/portfolio/{coinSymbol} — tek bir coinin özetini getir
    // Örnek: GET api/portfolio/BTC
    [HttpGet("{coinSymbol}")]
    public async Task<IActionResult> GetCoinPortfolio(string coinSymbol)
    {
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        // Service'ten tek coin portföyü hesapla
        var portfolio = await _portfolioservice.GetCoinPortfolioAsync(userId,coinSymbol);

        // Coin bulunamazsa 404 döndür
        if(portfolio == null)
            return NotFound(new {message =$"{coinSymbol} için işlem bulunamadı"});

        return Ok(portfolio);        
    }

}