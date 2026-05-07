using CryptoTracker.DTOs;

namespace  CryptoTracker.Services;

// ITransactionService → Transaction iş mantığının sözleşmesi
// Controller bu interface'i kullanır, içeride nasıl çalıştığını bilmez
public interface ITransactionService
{
    // Sayfalanmış ve filtrelenmiş işlem listesi
    // page → hangi sayfa, pageSize → sayfa başına kaç kayıt
    // coinSymbol → opsiyonel filtre, null ise tüm coinler
    // type → opsiyonel filtre, "buy" veya "sell", null ise hepsi
    Task<PaginatedResultDto<TransactionDto>> GetUserTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        string? coinSymbol,
        string? tpye);

    // Yeni işlem ekle — al veya sat
    // userId → işlemi kimin yaptığı
    // dto → kullanıcının gönderdiği işlem bilgileri
    Task<TransactionDto> AddTransactionAsync(string userId,CreateTransactionDto dto);

    // İşlemi sil
    // id → silinecek işlemin ID'si
    // userId → güvenlik için — sadece kendi işlemini silebilsin
    Task<bool> DeleteTransactionAsync(int id,string userId);
}