namespace CryptoTracker.DTOs;

// PortfolioSummaryDto → tüm portföyün genel özeti
// Dashboard'ın üstündeki stat kartlarında göstereceğiz
public class PortfolioSummaryDto
{
    // Toplam portföy değeri — tüm coinlerin güncel değeri toplamı
    public decimal TotalValue { get; set; }

    // Toplam yatırım — tüm alışların toplam tutarı
    public decimal TotalInvested { get; set; }

    // Toplam kar/zarar tutarı
    // TotalValue - TotalInvested
    public decimal TotalProfitLoss { get; set; }

    // Toplam kar/zarar yüzdesi
    public decimal TotalProfitLossPercentage { get; set; }

    // Kaç farklı coin var portföyde
    public int CoinCount { get; set; }

    // Her coinin detaylı özeti — dashboard tablosunda göstereceğiz
    public IEnumerable<PortfolioDto> Coins {get; set;} = new List<PortfolioDto>();
}