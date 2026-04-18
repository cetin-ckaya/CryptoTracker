using System.ComponentModel.DataAnnotations.Schema;
using CryptoTracker.Models;

namespace CryptoTracker.Models;

public class Transaction
{
    public int Id { get; set; }

    // Identity'de kullanıcı ID'si string (GUID formatında) — "abc123-..." gibi
    // Önceden int yazmıştık, Identity ile uyumsuzdu, string yaptık
    public string UserId { get; set; } = string.Empty;

    // Hangi coin — örneğin "BTC", "ETH", "SOL"
    public string CoinSymbol { get; set; } = string.Empty;

    // "buy" veya "sell" — alış mı satış mı
    public string Type { get; set; } = string.Empty;

    // Kaç coin alındı/satıldı — virgülden sonra 8 basamak (satoshi hassasiyeti)
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    // İşlem anındaki coin fiyatı — virgülden sonra 2 basamak yeterli
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Navigation property → bu işlemin sahibi olan kullanıcıya erişmek için
    public User User { get; set; } = null!;
} 