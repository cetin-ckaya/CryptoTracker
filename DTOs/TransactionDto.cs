namespace CryptoTracker.DTOs;


    // TransactionDto → Transaction modelinin dışarıya gönderilen versiyonu
    // UserId burada YOK — zaten giriş yapmış kullanıcının işlemlerini döneceğiz
    // User navigation property YOK — iç model detayları dışarı çıkmamalı
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

    // Toplam işlem tutarı — Amount * Price
    // get only → sadece okunabilir, veritabanında saklanmaz
    // AutoMapper bunu hesaplayarak doldurur
    public decimal Total => Amount * Price;

    }
