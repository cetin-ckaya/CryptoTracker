using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTracker.Models;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CoinSymbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}