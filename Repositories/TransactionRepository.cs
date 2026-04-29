using Microsoft.EntityFrameworkCore;
using CryptoTracker.Data;
using CryptoTracker.Models;

namespace CryptoTracker.Repositories;

// TransactionRepository → Transaction tablosundaki veritabanı işlemlerini yapar
// Tüm db.Transactions sorguları buraya toplanır
public class TransactionRepository : ITransactionRepository
{
    // AppDbContext → veritabanına erişmek için
    private readonly AppDbContext _context;

    // Constructor → AppDbContext dependency injection ile otomatik gelir
    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

     // Kullanıcıya ait tüm işlemleri tarihe göre sıralı getir
     public async Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId)
    {
        // Where → sadece bu kullanıcının işlemlerini filtrele
        // OrderByDescending → en yeni işlem en üstte görünsün
        // ToListAsync → sorguyu çalıştır ve listeye dönüştür
        return await _context.Transactions //bu nokta transaction ne amlama geliyor
        .Where(t => t.UserId == userId)    //t => ne anlama geliyor
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    }

    // Tek bir işlemi getir — hem ID hem userId kontrol edilir
    public async Task<Transaction?> GetByIdAsync(int id,string userId)
    {
        // FirstOrDefaultAsync → koşulu sağlayan ilk kaydı getir
        // Kayıt yoksa null döner (? işareti bunu ifade eder)
        // userId kontrolü → kullanıcı başkasının işlemine erişemesin
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    //Yeni işlem ekle
    public async Task<Transaction> AddAsync(Transaction transaction) //bu taskların içnie yazdıgımın transactiolar ne işe yarıyor
    {
        // _context.Transactions.Add → işlemi veritabanına eklemek için hazırla
        // Henüz veritabanına yazılmadı, sadece takibe alındı
        _context.Transactions.Add(transaction);

        // SaveChangesAsync → takibe alınan tüm değişiklikleri veritabanına yaz
        await _context.SaveChangesAsync();

        // Eklenen işlemi döndür — ID artık veritabanı tarafından atandı
        return transaction;
    }

    //İşlemi sil

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        // Önce işlemi bul — hem ID hem userId kontrol et
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        // İşlem bulunmadıysa false döndür
        if(transaction == null) return false;

        // İşlemi takipten çıkar — silinmek üzere işaretle
        _context.Transactions.Remove(transaction);

        // Değişikliği veritabanına yaz
        await _context.SaveChangesAsync();

        return true;
    }
}