namespace CryptoTracker.DTOs
{
    public class TransactionDto
    {
     // İşlemin ID'si — silme veya güncelleme için kullanacağız
    public int Id { get; set; }

    // Hangi coin — örneğin "BTC", "ETH"
    public string CoinSymbol { get; set; } = string.Empty;

    // Al mı sattı mı — "buy" veya "sell"
    public string Type { get; set; } = string.Empty;

    // Kaç coin alındı/satıldı — örneğin 0.5 BTC
    public decimal Amount { get; set; }

    // İşlem anındaki coin fiyatı — örneğin 43250.99
    public decimal Price { get; set; }

    // İşlem tarihi — portföy geçmişinde göstereceğiz
    public DateTime Date { get; set; }
    }
}