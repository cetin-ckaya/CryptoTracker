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
        var portfolio = await _portfolioservice.GetPortfolioAsync(userId);

        return Ok(portfolio);
    }

    // GET api/portfolio/summary — tüm portföyün genel özeti
    // Dashboard'ın stat kartları bu endpoint'ten beslenecek
    [HttpGet("{summary}")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        //Serviceten genel özeti al
        var summary = await _portfolioservice.GetPortfolioSummaryAsync(userId);

        return Ok(summary);
    }

    // GET api/portfolio/{coinSymbol} — tek bir coinin özetini getir
    // Örnek: GET api/portfolio/BTC
    [HttpGet("coin/{coinSymbol}")]
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