using System.Transactions;
using CryptoTracker.DTOs;

namespace  CryptoTracker.Services;

// ITransactionService → Transaction iş mantığının sözleşmesi
// Controller bu interface'i kullanır, içeride nasıl çalıştığını bilmez
public interface ITransactionService
{
    // Kullanıcıya ait tüm işlemleri getir
    // userId → hangi kullanıcının işlemleri isteniyor
    Task<IEnumerable<TransactionDto>> GetUserTransactionAsync(string userId);  //task ne işe yarıyor, IEnumarable ne işe yarıyor içine yazdığımız paramereyi ne yapıyor ve getusertransactionasync bizim oluşturdğumuz bir method mu?

    // Yeni işlem ekle — al veya sat
    // userId → işlemi kimin yaptığı
    // dto → kullanıcının gönderdiği işlem bilgileri
    Task<TransactionDto> AddTransactionAsync(string userId,CreateTransactionDto dto);

    // İşlemi sil
    // id → silinecek işlemin ID'si
    // userId → güvenlik için — sadece kendi işlemini silebilsin
    Task<bool> DeleteTransactionAsync(int id,string userId);
}