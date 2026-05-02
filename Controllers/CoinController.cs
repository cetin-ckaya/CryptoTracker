using CryptoTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTracker.Controllers;

// CoinController → anlık coin fiyatlarını döndürür
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoinController : ControllerBase
{
    // ICoinService → CoinGecko'dan fiyat çekmek için
    private readonly ICoinService _coinservice;

    public CoinController(ICoinService coinService)
    {
        _coinservice = coinService;
    }

    // GET api/coin/prices?symbols=BTC,ETH,SOL
    // Query parametresi ile birden fazla coin fiyatı isteniyor
    [HttpGet("prices")]
    public async Task<IActionResult> GetPrices([FromQuery] string symbols)
    {
        // symbols → "BTC,ETH,SOL" formatında geliyor
        // Split → virgülle ayırıp listeye çevir
        var symbolList = symbols.Split(",").Select(s => s.Trim()).ToList();

        //CoinGecko dan fiyatlari çek
        var prices = await _coinservice.GetCoinPriceAsync(symbolList);

        return Ok(prices);
    } 


    // GET api/coin/price/BTC — tek coin fiyatı
    [HttpGet("price/{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        // CoinGecko'dan tek coin fiyatı çek
        var price = await _coinservice.GetCoinPriceAsync(symbol);

        //fiyat bulunamazsa 404 döndür
        if(price == null)
            return NotFound(new {message = $"{symbol} için fiyat bulunamadı"});
        
        return Ok(new {symbol = symbol.ToUpper(), price});
    }
}