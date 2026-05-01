using AutoMapper;
using CryptoTracker.DTOs;
using CryptoTracker.Models;
using CryptoTracker.Repositories;

namespace CryptoTracker.Services;

// TransactionService → Transaction iş mantığını taşır
// Repository üzerinden veritabanıyla konuşur
// Controller sadece bu service'i çağırır
public class TransactionService : ITransactionService
{
    // ITransactionRepository → veritabanı işlemleri için
    private readonly ITransactionRepository _transactionrepository;

    // IMapper → Transaction → TransactionDto dönüşümü için
    private readonly IMapper _mapper;

    public TransactionService(ITransactionRepository transactionrepository, IMapper mapper)
    {
        _transactionrepository = transactionrepository;
        _mapper = mapper;
    }
    // Kullanıcıya ait tüm işlemleri getir ve DTO'ya dönüştür
    public async Task<IEnumerable<TransactionDto>> GetUserTransactionAsync(string userId)
    {
        // Repository'den işlemleri al
        var transaction = await _transactionrepository.GetByUserIdAsync(userId);

        // Transaction listesini TransactionDto listesine dönüştür
        // IEnumerable<Transaction> → IEnumerable<TransactionDto>
        return _mapper.Map<IEnumerable<TransactionDto>>(transaction);
    }

    // Yeni işlem ekle
    public async Task<TransactionDto> AddTransactionAsync(string userId,CreateTransactionDto dto)
    {
        // DTO'dan Transaction modeli oluştur
        var transaction = new Transaction
        {
            // Giriş yapmış kullanıcının ID'si — token'dan gelecek
            UserId = userId,

            // Kullanıcının gönderdiği bilgiler
            CoinSymbol = dto.CoinSymbol.ToUpper(),
            Type = dto.Type.ToLower(),
            Amount = dto.Amount,
            Price = dto.Price,

            // İşlem tarihini otomatik set et — kullanıcı göndermez
            Date = DateTime.UtcNow
        };

        // Repository'ye ekle — veritabanına kaydeder
        var added = await _transactionrepository.AddAsync(transaction);//addasync ne işe yarıyor içine yazdığımız parametreyi ne yapıyor
    
        // Eklenen Transaction'ı TransactionDto'ya dönüştür ve döndür
        return _mapper.Map<TransactionDto>(added);
    }

    //İşlemi Sil
    public async Task<bool> DeleteTransactionAsync(int id,string userId)
    {
        // Repository'ye silme isteği gönder
        // Repository hem ID hem userId kontrol eder — başkasının işlemini silemez
        return await _transactionrepository.DeleteAsync(id,userId);
    }
}