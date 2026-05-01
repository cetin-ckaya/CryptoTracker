using CryptoTracker.DTOs;

namespace CryptoTracker.Services;

// IPortfolioService → portfolio hesaplama iş mantığının sözleşmesi
public interface IPortfolioService
{
    // Kullanıcının tüm coinlerinin portföy özetini getir
    // Her coin için ayrı bir PortfolioDto döner
    Task<IEnumerable<PortfolioDto>> GetPortfoliosAsync(string userId);

    // Tek bir coinin portföy özetini getir
    // Örnek: sadece BTC'nin durumunu görmek için
    Task<PortfolioDto?> GetCoinPortfolioAsync(string userId,string coinSymbol);
}