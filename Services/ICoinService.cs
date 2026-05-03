namespace CryptoTracker.Services;

// ICoinService → dış API'den coin fiyatlarını çeken servisin sözleşmesi
// CoinGecko API'yi kullanacağız — ücretsiz, kayıt gerektirmez
public interface ICoinService
{
    // Tek bir coinin anlık fiyatını getir
    // coinSymbol → "BTC", "ETH" gibi
    // decimal? → fiyat bulunamazsa null döner
    Task<decimal?> GetCoinPriceAsync(string coinSymbol);

    // Birden fazla coinin fiyatını tek seferde getir
    // Örnek: BTC, ETH, SOL fiyatlarını aynı anda çek
    // Dictionary → her coin sembolü için bir fiyat döner
    Task<Dictionary<string, decimal>> GetCoinPricesAsync(IEnumerable<string> coinSymbols);
}