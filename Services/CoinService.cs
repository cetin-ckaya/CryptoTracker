using System.Text.Json;

namespace CryptoTracker.Services;

// CoinService → CoinGecko API'den anlık coin fiyatlarını çeker
// HttpClient → dış API'ye HTTP isteği atmak için kullanıyoruz
public class CoinService : ICoinService
{   
    // HttpClient → CoinGecko API'ye istek atmak için
    private readonly HttpClient _httpclient;

    // Sembol → CoinGecko ID eşleştirmesini cache'de tutuyoruz
    // Uygulama her başladığında CoinGecko'dan çekiyoruz
    // Dictionary → "BTC" → "bitcoin" gibi
    private static Dictionary<string,string> _coinIds = new();

    // Cache ne zaman dolduruldu — 1 saatte bir yeniliyoruz
    // static → tüm instance'lar aynı cache'i paylaşır
    // Singleton olmadan da cache çalışır
    private static DateTime _lastCacheTime = DateTime.MinValue;

    public CoinService(HttpClient httpClient)
    {
        _httpclient = httpClient;
         // HttpClient Program.cs'de yapılandırıldı, burada sadece atıyoruz
       
    }

    // CoinGecko'daki tüm coinlerin sembol → ID eşleştirmesini çek
    // Cache 1 saatten eskiyse yenile, değilse mevcut cache'i kullan
    private async Task EnsureCoinIdsLoadedAsync()
    {
        // Cache doluysa ve 1 saatten yeni ise tekrar çekme
        if(_coinIds.Any() && DateTime.UtcNow - _lastCacheTime < TimeSpan.FromHours(1))
            return;

        try
        {
            // CoinGecko'dan tüm coin listesini çek
            // Örnek yanıt: [{"id":"bitcoin","symbol":"btc","name":"Bitcoin"}, ...]
            var response = await _httpclient.GetStringAsync("coins/list");
            var json = JsonDocument.Parse(response);

            // Önce bilinen popüler coinleri manuel olarak tanımla
            // Aynı sembolle birden fazla coin olduğunda doğru olanı seçmek için
            var knowCoins = new Dictionary<string,string>
            {
                { "BTC", "bitcoin" },
                { "ETH", "ethereum" },
                { "SOL", "solana" },
                { "BNB", "binancecoin" },
                { "ADA", "cardano" },
                { "XRP", "ripple" },
                { "DOGE", "dogecoin" },
                { "DOT", "polkadot" },
                { "AVAX", "avalanche-2" },
                { "MATIC", "matic-network" },
                { "LINK", "chainlink" },
                { "UNI", "uniswap" },
                { "LTC", "litecoin" },
                { "ATOM", "cosmos" },
                { "TRX", "tron" }
            
            };

            // CoinGecko listesinden geri kalanları ekle
            // Ama bilinen coinlerin üzerine yazma
            var allCoins = json.RootElement
            .EnumerateArray()
            .GroupBy(c => c.GetProperty("symbol").GetString()!.ToUpper())
            .ToDictionary(
                g => g.Key,
                g => g.First().GetProperty("id").GetString()!
            );

            // Önce tüm listeyi ekle, sonra bilinen coinlerle üzerine yaz
            _coinIds = allCoins;
            foreach (var (symbol, id) in knowCoins)
                _coinIds[symbol] = id;


            
            // Cache zamanını güncelle
            _lastCacheTime = DateTime.UtcNow;
        }
        catch
        {
            // Hata olursa mevcut cache'i kullanmaya devam et
        }
    }

    // Tek bir coinin anlık fiyatını getir
    public async Task<decimal?> GetCoinPriceAsync(string coinSymbol)
    {
        // Cache'i kontrol et, gerekirse yenile
        await EnsureCoinIdsLoadedAsync();

        var symbol = coinSymbol.ToUpper();

        //Sembol listede yoksa null döndür
        if(!_coinIds.TryGetValue(symbol, out var coinId))
            return null;

        try
        {
            // CoinGecko API'ye istek at
            // simple/price → basit fiyat endpoint'i
            // ids → hangi coin, vs_currencies → hangi para birimi (usd)
            var url = $"simple/price?ids={coinId}&vs_currencies=usd";
            var response = await _httpclient.GetStringAsync(url);

            // Gelen JSON'ı parse et
            // Örnek JSON: {"bitcoin":{"usd":67420.5}}
            var json = JsonDocument.Parse(response);

            // JSON içinden fiyatı çıkar
            return json.RootElement
                .GetProperty(coinId)    // "bitcoin" anahtarına git
                .GetProperty("usd")     // "usd" anahtarına git
                .GetDecimal();          // decimal değeri al
        }
        catch
        {   
            // API isteği başarısız olursa null döndür
            // Örnek: internet yok, API limit aşıldı
            return null;
        }
    }

    //Birden fazla coinin fiyatını tek seferde getir 
    public async Task<Dictionary<string,decimal>> GetCoinPricesAsync(IEnumerable<string> coinSymbols)
    {
        //Cachi kontrol et, gerekirse yenile
        await EnsureCoinIdsLoadedAsync();

        // Sonuç dictionary'si — sembol → fiyat
        var result = new Dictionary<string,decimal>();

        // Sembolü büyük harfe çevir ve listemizdeki coinleri filtrele
        var symbols = coinSymbols.Select(s => s.ToUpper()).ToList();

        //Her sembol için coingecko Id sini bul
        // Listemizdê olmayan coinleri atla
        var coinIdMap = symbols
        .Where(s => _coinIds.ContainsKey(s))
        .ToDictionary(s => s, s=> _coinIds[s]);

        // Hiç geçerli coin yoksa boş dictionary döndür
        if(!coinIdMap.Any()) return result;

        try
        {
            // Tüm coin ID'lerini virgülle birleştir
            // Örnek: "bitcoin,ethereum,solana"
            var ids = string.Join(",", coinIdMap.Values);

            // CoinGecko'ya tek istekte tüm fiyatları iste
            var url = $"simple/price?ids={ids}&vs_currencies=usd";
            var response = await _httpclient.GetStringAsync(url);

            // Gelen JSON'ı parse et
            var json = JsonDocument.Parse(response);

            // Her sembol için fiyatı çıkar ve dictionary'e ekle
            foreach(var (symbol, coinId) in coinIdMap)
            {
                // JSON içinde bu coin var mı kontrol et
                if(json.RootElement.TryGetProperty(coinId, out var coinData))
                {
                    // USD fiyatını al
                    if(coinData.TryGetProperty("usd", out var priceElement))
                    {
                        // Sembol → fiyat olarak kaydet
                        result[symbol] = priceElement.GetDecimal();
                    }
                }
            }
        }
        catch
        {
            // Hata olursa boş dictionary döndür
        }
        return result;
    }
}