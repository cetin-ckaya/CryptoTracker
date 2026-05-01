namespace CryptoTracker.DTOs;

// PortfolioDto → tek bir coinin portföy özetini taşır
// Dashboard'da her coin için bu bilgileri göstereceğiz
public class PortfolioDto
{
    // Hangi coin — "BTC", "ETH", "SOL"
    public string CoinSymbol { get; set; } = string.Empty;

    // Toplam kaç coin elimizde var
    // Alımlar toplamı - Satımlar toplamı
    public decimal TotalAmount { get; set; }

    // Ortalama alış maliyeti
    // Tüm alımların toplam tutarı / toplam alınan coin miktarı
    public decimal AverageBuyPrice { get; set; }

    // Toplam yatırım tutarı — sadece alışlar hesaplanır
    // Örnek: 0.5 BTC * $60,000 = $30,000
    public decimal TotalInvested { get; set; }

    // Güncel toplam değer — anlık fiyat ile hesaplanır
    // Gün 11'de CoinGecko'dan fiyat çekince doldurulacak
    // Şimdilik 0 — ilerleyen günlerde güncelleyeceğiz
    public decimal CurrentValue { get; set; }

    // Kar veya zarar tutarı
    // CurrentValue - TotalInvested
    public decimal ProfitLoss { get; set; }

    // Kar/zarar yüzdesi
    // (ProfitLoss / TotalInvested) * 100
    public decimal ProfitLossPercentage { get; set; }
}