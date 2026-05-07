namespace CryptoTracker.DTOs;

// PaginatedResultDto → sayfalanmış sonuçları taşır
// T → generic tip, her türlü liste için kullanılabilir
// Örnek: PaginatedResultDto<TransactionDto>
public class PaginatedResultDto<T>
{
    // Toplam kayıt sayısı — "150 işlemden 10 tanesi gösteriliyor" için
    public int TotalCount { get; set; }

    // Toplam sayfa sayısı — TotalCount / PageSize
    public int TotalPages { get; set; }

    // Şu anki sayfa numarası
    public int CurrentPage { get; set; }

    // Sayfa başına kayıt sayısı
    public int PageSize { get; set; }

    // O sayfadaki veriler
    public IEnumerable<T> Data { get; set; } = new List<T>();
}