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
        // Bir coinin tüm işlemlerinden portföy hesaplayan yardımcı metod
    private PortfolioDto CalculatePortfolio(string coinSymbol, List<Models.Transaction> transactions, decimal currentPrice)
    {
        // Büyük/küçük harf duyarlılığını ortadan kaldırmak için ToLower() kullanıyoruz
        var buyTransactions = transactions.Where(t => t.Type.ToLower() == "buy").ToList();
        var sellTransactions = transactions.Where(t => t.Type.ToLower() == "sell").ToList();

        // 1. Miktar Hesaplaması
        var totalBought = buyTransactions.Sum(t => t.Amount);
        var totalSold = sellTransactions.Sum(t => t.Amount);
        var totalAmount = totalBought - totalSold;

        // 2. Alışlara ödenen toplam para ve Ortalama Maliyet
        var totalPaid = buyTransactions.Sum(t => t.Amount * t.Price);
        var averageBuyPrice = totalBought > 0 ? totalPaid / totalBought : 0;

        // 3. ŞU AN ELİMİZDE KALAN coinlerin maliyeti (Dashboard'da Toplam Yatırım olarak görünen)
        var totalInvested = totalAmount * averageBuyPrice;

        // 4. Satışlardan elde edilen toplam gelir
        var totalMoneyReceived = sellTransactions.Sum(t => t.Amount * t.Price);

        // 5. Güncel Değer (Elimizdeki coinlerin anlık fiyat karşılığı)
        var currentValue = totalAmount * currentPrice;

        // 6. Toplam Kar/Zarar 
        // Formül: (Şu anki değer + Satıştan gelen para) - Alışa ödenen toplam para
        var profitLoss = (currentValue + totalMoneyReceived) - totalPaid;

        // 7. Kar/Zarar Yüzdesi (Tüm yatırıma oranla)
        var profitLossPercentage = totalPaid > 0 ? (profitLoss / totalPaid) * 100 : 0;

        return new PortfolioDto
        {
            CoinSymbol = coinSymbol,
            TotalAmount = totalAmount,
            AverageBuyPrice = averageBuyPrice,
            TotalInvested = totalInvested, // Kalan coinlerin maliyeti yansıyacak
            CurrentValue = currentValue,
            ProfitLoss = profitLoss,       // Satış karları da dahil gerçek kâr!
            ProfitLossPercentage = profitLossPercentage,
            CurrentPrice = currentPrice
        }; 
    }
}