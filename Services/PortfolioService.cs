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

    // ICoinService → anlık coin fiyatlarını çekmek için
    private readonly ICoinService _coinservice;

    public PortfolioService(ITransactionRepository transactionRepository,ICoinService coinService)
    {
        _transactionrepository = transactionRepository;
        _coinservice = coinService;
    }

    // Kullanıcının tüm coinlerinin portföy özetini hesapla
    public async Task<IEnumerable<PortfolioDto>> GetPortfolioAsync(string userId)
    {
        // Kullanıcının tüm işlemlerini getir
        var transaction = await _transactionrepository.GetByUserIdAsync(userId);


        // İşlemleri coin sembolüne göre grupla
        // Örnek: BTC işlemleri bir grup, ETH işlemleri başka bir grup
        var grouped = transaction.GroupBy(t => t.CoinSymbol);

        // Elimizdeki tüm coin sembollerini topla
        var symbols = grouped.Select(g => g.Key).ToList();

        // CoinGecko'dan tüm coinlerin fiyatını tek seferde çek
        // Tek tek istek atmak yerine toplu istek daha verimli
        var prices = await _coinservice.GetCoinPricesAsync(symbols);

        // Her coin grubu için PortfolioDto hesapla
        var portfolio = grouped.Select(group =>
        {
            // Bu coin için anlık fiyat var mı kontrol et
            // Yoksa 0 kullan
            var currenPrice = prices.TryGetValue(group.Key, out var price) ? price : 0;
            
            // Portfolio hesapla — anlık fiyatı da gönder
            return CalculatePortfolio(group.Key, group.ToList(), currenPrice);

        });

        //Sadece elimizde coin olanları döndür
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

        var currenPrice = await _coinservice.GetCoinPriceAsync(coinSymbol) ?? 0;

        //Portfolio hesapla ve döndür
        return CalculatePortfolio(coinSymbol.ToUpper(),coinTransactions, currenPrice); 
    }

    //Tüm portföyün genel özetini hesapla
    public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId)
    {
        // Tüm coinlerin portföy özetini al — anlık fiyatlar dahil
        var portfolio = (await GetPortfolioAsync(userId)).ToList();

        // Toplam portföy değeri — tüm coinlerin güncel değerlerini topla
        var totalValue = portfolio.Sum(p => p.CurrentValue);

        // Toplam yatırım — tüm coinlerin yatırım tutarlarını topla
        var totalInvested = portfolio.Sum(p => p.TotalInvested);

        //Toplam Kar/zarar
        var totalProfitLoss = totalValue - totalInvested;

        //Toplam Kar/zarar yüzdesi
        var totalProfitLossPercentage = totalInvested > 0 ?
            (totalProfitLoss / totalInvested) * 100 : 0;

        return new PortfolioSummaryDto
        {
            TotalValue = totalValue,
            TotalInvested = totalInvested,
            TotalProfitLoss = totalProfitLoss,
            TotalProfitLossPercentage = totalProfitLossPercentage,

            //Kaç farklı coin var
            CoinCount = portfolio.Count,

            //Her coinin detaylı özeti
            Coins = portfolio

        };
    }

    // Bir coinin tüm işlemlerinden portföy hesaplayan yardımcı metod
    private PortfolioDto CalculatePortfolio(string coinSymbol, List<Models.Transaction> transactions, decimal currentPrice) // list models transaction ne demek
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
        // elimizdeki miktar * anlık fiyat = güncel değer
        var currentValue = totalAmount * currentPrice;

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