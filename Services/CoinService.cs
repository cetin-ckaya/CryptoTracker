using System.Text.Json;
using System.Collections.Concurrent;

namespace CryptoTracker.Services;

public class CoinService : ICoinService
{   
    private readonly HttpClient _httpclient;
    private static Dictionary<string,string> _coinIds = new();
    private static DateTime _lastCacheTime = DateTime.MinValue;

    // Fiyat Cache'i - Başarıyla çekilen tüm coinlerin fiyatlarını tutar
    private static ConcurrentDictionary<string, decimal> _priceCache = new();
    
    // 🔥 YENİ: Cache zamanı - Her coinin fiyatının ne zaman çekildiğini tutar
    private static ConcurrentDictionary<string, DateTime> _priceCacheTime = new();

    public CoinService(HttpClient httpClient)
    {
        _httpclient = httpClient;
        
        if (!_httpclient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "CryptoTrackerApp/1.0");
        }
    }

private async Task EnsureCoinIdsLoadedAsync()
{
    if(_coinIds.Any() && DateTime.UtcNow - _lastCacheTime < TimeSpan.FromHours(1))
        return;

    try
    {
        // 🔥 TAM URL - BaseAddress gerekmez
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoTracker/1.0");
        
        var response = await httpClient.GetStringAsync(
            "https://api.coingecko.com/api/v3/coins/list"
        );
        
        Console.WriteLine($"✅ Coin listesi çekildi, uzunluk: {response.Length}");
        
        var json = JsonDocument.Parse(response);

        var knowCoins = new Dictionary<string,string>
        {
            { "BTC", "bitcoin" }, { "ETH", "ethereum" }, { "SOL", "solana" },
            { "BNB", "binancecoin" }, { "ADA", "cardano" }, { "XRP", "ripple" },
            { "DOGE", "dogecoin" }, { "DOT", "polkadot" }, { "AVAX", "avalanche-2" },
            { "MATIC", "matic-network" }, { "LINK", "chainlink" }, { "UNI", "uniswap" },
            { "LTC", "litecoin" }, { "ATOM", "cosmos" }, { "TRX", "tron" },
            { "EIGEN", "eigenlayer" }, { "METIS", "metis-token" },
            { "PEPE", "pepe" }, { "SHIB", "shiba-inu" }
        };

        var allCoins = json.RootElement
        .EnumerateArray()
        .GroupBy(c => c.GetProperty("symbol").GetString()!.ToUpper())
        .ToDictionary(
            g => g.Key,
            g => g.First().GetProperty("id").GetString()!
        );

        _coinIds = allCoins;
        foreach (var (symbol, id) in knowCoins)
            _coinIds[symbol] = id;
        
        _lastCacheTime = DateTime.UtcNow;
        Console.WriteLine($"✅ {_coinIds.Count} adet coin ID yüklendi");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Coin ID yükleme hatası: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"🔍 Inner: {ex.InnerException.Message}");
        }
    }
}

public async Task<decimal?> GetCoinPriceAsync(string coinSymbol)
{
    await EnsureCoinIdsLoadedAsync();
    var symbol = coinSymbol.ToUpper();

    if(!_coinIds.TryGetValue(symbol, out var coinId))
    {
        Console.WriteLine($"❌ Coin ID bulunamadı: {symbol}");
        return null;
    }

    try
    {
        // 🔥 TAM URL Kullan
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoTracker/1.0");
        
        var url = $"https://api.coingecko.com/api/v3/simple/price?ids={coinId}&vs_currencies=usd";
        Console.WriteLine($"🌐 {url}");
        
        var response = await httpClient.GetStringAsync(url);
        Console.WriteLine($"✅ Yanıt: {response}");
        
        var json = JsonDocument.Parse(response);

        var price = json.RootElement
            .GetProperty(coinId)    
            .GetProperty("usd")     
            .GetDecimal();          

        _priceCache[symbol] = price;
        _priceCacheTime[symbol] = DateTime.UtcNow;
        
        Console.WriteLine($"✅ {symbol} fiyatı: ${price}");

        return price;
    }
    catch (Exception ex)
    {   
        Console.WriteLine($"❌ {symbol} API hatası: {ex.Message}");
        
        if (_priceCache.TryGetValue(symbol, out var cachedPrice))
        {
            Console.WriteLine($"📦 {symbol} cache'den: ${cachedPrice}");
            return cachedPrice;
        }

        return null;
    }
}

    public async Task<Dictionary<string,decimal>> GetCoinPricesAsync(IEnumerable<string> coinSymbols)
    {
        await EnsureCoinIdsLoadedAsync();
        var result = new Dictionary<string,decimal>();
        var symbols = coinSymbols.Select(s => s.ToUpper()).ToList();

        var coinIdMap = symbols
        .Where(s => _coinIds.ContainsKey(s))
        .ToDictionary(s => s, s=> _coinIds[s]);

        if(!coinIdMap.Any()) return result;

        try
        {
            var ids = string.Join(",", coinIdMap.Values);
            var url = $"simple/price?ids={ids}&vs_currencies=usd";
            
            var response = await _httpclient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            foreach(var (symbol, coinId) in coinIdMap)
            {
                if(json.RootElement.TryGetProperty(coinId, out var coinData))
                {
                    if(coinData.TryGetProperty("usd", out var priceElement))
                    {
                        var price = priceElement.GetDecimal();
                        result[symbol] = price;

                        // Cache'le
                        _priceCache[symbol] = price;
                        _priceCacheTime[symbol] = DateTime.UtcNow;
                        
                        Console.WriteLine($"✅ {symbol} fiyatı çekildi: ${price}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Toplu fiyat çekme hatası: {ex.Message}");
            
            // Her coin için cache'den dene
            foreach (var symbol in coinIdMap.Keys)
            {
                if (_priceCache.TryGetValue(symbol, out var cachedPrice))
                {
                    result[symbol] = cachedPrice;
                    Console.WriteLine($"📦 {symbol} cache'den dönüldü: ${cachedPrice}");
                }
                else
                {
                    result[symbol] = 0;
                    Console.WriteLine($"⚠️ {symbol} için cache boş, 0 dönüldü");
                }
            }
        }
        
        return result;
    }
}