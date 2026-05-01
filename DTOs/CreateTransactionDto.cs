namespace CryptoTracker.DTOs;

// CreateTransactionDto → kullanıcı yeni işlem eklerken gönderdiği veri
// Id, UserId, Date burada YOK — bunları biz otomatik set edeceğiz
public class CreateTransactionDto
{
    // Hangi coin — "BTC", "ETH", "SOL" gibi
    public string CoinSymbol { get; set; } = string.Empty;

    // İşlem tipi — "buy" veya "sell"
    public string Type { get; set; } = string.Empty;

    // Kaç coin alındı/satıldı — örneğin 0.5 BTC
    public decimal Amount { get; set; }

    // İşlem anındaki fiyat — örneğin 67000
    public decimal Price { get; set; }
    
}