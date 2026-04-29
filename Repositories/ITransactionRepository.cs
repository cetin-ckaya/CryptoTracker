using CryptoTracker.Models;

namespace CryptoTracker.Repositories;

// Service bu interface'i kullanır, içeride nasıl çalıştığını bilmez
public interface ITransactionRepository
{
    // Kullanıcıya ait tüm işlemleri getir
    // userId → hangi kullanıcının işlemleri isteniyor
    Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId);

    // Tek bir işlemi ID ile getir
    // id → işlemin veritabanı ID'si
    // userId → güvenlik için — sadece kendi işlemini görebilsin
    Task<Transaction?> GetByIdAsync(int id, string userId);

    // Yeni işlem ekle — al veya sat
    Task<Transaction> AddAsync(Transaction transaction);

    // İşlemi sil
    // Geriye bool döner — silme başarılıysa true, işlem bulunamazsa false
    Task<bool> DeleteAsync(int id,string userId);
}