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
    // Sayfalanmış ve filtrelenmiş işlem listesi
    public async Task<PaginatedResultDto<TransactionDto>> GetUserTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        string? coinSymbol,
        string? type)
    {
        // Repository'den kullanıcının tüm işlemlerini al
        var transactions = await _transactionrepository.GetByUserIdAsync(userId);

        // Coin sembolüne göre filtrele — null ise filtreleme
        if (!string.IsNullOrEmpty(coinSymbol))
            transactions = transactions.Where(t => t.CoinSymbol == coinSymbol.ToUpper());
        
        //İşlem Tipine göre filtrele - buy veya sell
        if(!string.IsNullOrEmpty(type))
            transactions = transactions.Where(t => t.Type == type.ToUpper());

        //Toplam kayıt sayısı - filtrelenmiş sonuçtan
        var totalCount = transactions.Count();

        // Toplam sayfa sayısı — Math.Ceiling yuvarlama yapar
        // Örnek: 15 kayıt / 10 = 1.5 → 2 sayfa
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        // Sayfalama — kaç kayıt atlayacağız
        // Örnek: sayfa 2, pageSize 10 → 10 kayıt atla
        var data = transactions
            .Skip((page - 1)* pageSize)     // (2-1) * 10 = 10 kayıt atla
            .Take(pageSize)                 // sonraki 10 kaydı al
            .ToList();


        //DTO ya dönüştür
        var dtoList = _mapper.Map<IEnumerable<TransactionDto>>(data);

        return new PaginatedResultDto<TransactionDto>
        {
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Data = dtoList
        };
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