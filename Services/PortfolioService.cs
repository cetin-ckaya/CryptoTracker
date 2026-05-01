using System.Text.RegularExpressions;
using CryptoTracker.DTOs;
using CryptoTracker.Repositories;


namespace CryptoTracker.Services;

// PortfolioService → kullanıcının işlemlerinden portföy hesaplar
// Ortalama maliyet, toplam yatırım, kar/zarar burаdа hesaplanır
public class PortfolioService : IPortfolioService
{
    // ITransactionRepository → kullanıcının işlemlerini getirmek için
    private readonly ITransactionRepository _transactionrepository;

    public PortfolioService(ITransactionRepository transactionRepository)
    {
        _transactionrepository = transactionRepository;
    }

    // Kullanıcının tüm coinlerinin portföy özetini hesapla
    public async Task<IEnumerable<PortfolioDto>> GetPortfoliosAsync(string userId)
    {
        // Kullanıcının tüm işlemlerini getir
        var transaction = await _transactionrepository.GetByUserIdAsync(userId);


        // İşlemleri coin sembolüne göre grupla
        // Örnek: BTC işlemleri bir grup, ETH işlemleri başka bir grup
        var grouped = transaction.GroupBy(t => t.CoinSymbol);

        // Her coin grubu için PortfolioDto hesapla
        var portfolio = grouped.Select(group => CalculatePortfolio(group.Key, group.ToList()));
    
        // Sadece hâlâ elimizde coin olanları döndür
        // TotalAmount <= 0 olan coinleri listeden çıkar
        return portfolio.Where(p => p.TotalAmount > 0);
    }


    // Tek bir coinin portföy özetini hesapla
    public async Task<PortfolioDto> GetCoinPortfolioAsync(string userId,string coinSymbol)
    {
        // Kullanıcının tüm işlemlerini getir
        var transaction = await _transactionrepository.GetByUserIdAsync(userId);

        // Sadece istenen coinin işlemlerini filtrele
        // ToUpper() → "btc" ve "BTC" aynı şekilde aransın
        var coinTransactions = transaction
            .Where(t => t.CoinSymbol == coinSymbol.ToUpper())
            .ToList();

        //Hiç işlem yoksa null döndür
        if(!coinTransactions.Any()) return null;

        //Portfolio hesapla ve döndür
        return CalculatePortfolio(coinSymbol.ToUpper(),coinTransactions); 
    }

    // Bir coinin tüm işlemlerinden portföy hesaplayan yardımcı metod
    private PortfolioDto CalculatePortfolio(string coinSymbol, List<Models.Transaction> transactions) // list models transaction ne demek
    {
        //Sadece alış işlemlerini al
        var buyTransaction = transactions.Where(t => t.Type == "buy").ToList();

        //Sadece satış işlemlerini al
        var sellTransaction = transactions.Where(t =>t.Type == "sell").ToList();

        // Toplam alınan coin miktarı
        // Örnek: 0.5 BTC + 0.3 BTC = 0.8 BTC alındı
        var totalBought = buyTransaction.Sum(t => t.Amount);

        // Toplam satılan coin miktarı
        var totalSold = sellTransaction.Sum(t => t.Amount);

        // Elimizdeki coin miktarı
        // Alınan - Satılan = Kalan
        var totalAmount = totalBought - totalSold;

        // Toplam yatırım tutarı — her alışın tutarını topla
        // Örnek: 0.5 BTC * $60,000 + 0.3 BTC * $65,000 = $49,500
        var totalInvested = buyTransaction.Sum(t => t.Amount * t.Price);

        // Ortalama alış fiyatı
        // Toplam yatırım / toplam alınan miktar
        // Örnek: $49,500 / 0.8 BTC = $61,875 ortalama maliyet
        var averageBuyPrice = totalBought > 0 ? totalInvested / totalBought : 0;

        // Şu an için CurrentValue = 0
        // Gün 11'de CoinGecko'dan anlık fiyat çekince güncellenecek
        var currentValue = 0m;

        // Kar/zarar = Güncel değer - Toplam yatırım
        var profitLoss = currentValue - totalInvested;

        // Kar/zarar yüzdesi
        // Sıfıra bölme hatasını önlemek için totalInvested > 0 kontrolü
        var profitLossPercentage = totalInvested > 0 ? (profitLoss / totalInvested) * 100 : 0;


        return new PortfolioDto
        {
            CoinSymbol = coinSymbol,
            TotalAmount = totalAmount,
            AverageBuyPrice = averageBuyPrice,
            TotalInvested = totalInvested,
            CurrentValue = currentValue,
            ProfitLoss = profitLoss,
            ProfitLossPercentage = profitLossPercentage
        }; //yazmamızdaki amaç ne ? ne işe yaradı
    }
}