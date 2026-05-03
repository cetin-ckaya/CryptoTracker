using CryptoTracker.DTOs;

namespace CryptoTracker.Services;

// IPortfolioService → portfolio hesaplama iş mantığının sözleşmesi
public interface IPortfolioService
{
    // Kullanıcının tüm coinlerinin portföy özetini getir
    // Her coin için ayrı bir PortfolioDto döner
    Task<IEnumerable<PortfolioDto>> GetPortfolioAsync(string userId);

    // Tek bir coinin portföy özetini getir
    // Örnek: sadece BTC'nin durumunu görmek için
    Task<PortfolioDto?> GetCoinPortfolioAsync(string userId,string coinSymbol);

    // Tüm portföyün genel özetini getir — dashboard stat kartları için
    Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId);
}