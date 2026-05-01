using CryptoTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace CryptoTracker.Data;

// SeedData → uygulama ilk başladığında veritabanına örnek veri ekler
// Geliştirme aşamasında test için kullanışlı
// Her başlatmada kontrol eder — veri varsa tekrar eklemez
public static class SeedData
{
    public static async Task InitializeAsync( //InitializeAsync — uygulama ilk başladığında veritabanına örnek veri ekleyen bir metod.
        UserManager<User> userManager, // Identity'nin kullanıcı yönetim servisi
        AppDbContext context)          // Veritabanı bağlantısı
    {
        // Test kullanıcısı zaten varsa hiçbir şey yapma
        if (userManager.Users.Any()) return;

        // Test kullanıcısı oluştur
        var testUser = new User
        {   
            UserName = "testuser",
            Email = "test@cryptotracker.com",
            CreatedAt = DateTime.UtcNow
        };


        // Kullanıcıyı Identity ile oluştur — şifreyi otomatik hash'ler
        await userManager.CreateAsync(testUser,"Test123");

        // Test kullanıcısının ID'sini al
        var userId = testUser.Id;

        // Kullanıcının işlemleri zaten varsa ekleme
        if(context.Transactions.Any()) return;

        // Örnek işlemler ekle
        var transactions = new List<Transaction>
        {
          new Transaction
          {
              UserId = userId,
              CoinSymbol = "BTC",       // Bitcoin aldık
              Type = "buy",
              Amount = 0.5m,            // 0.5 BTC
              Price = 60000m,           // $60,000'den
              Date = DateTime.UtcNow.AddDays(-30)
          },

          new Transaction
          {
            UserId = userId,
            CoinSymbol = "ETH",
            Type = "buy",
            Amount = 2m,
            Price = 3000m,
            Date = DateTime.UtcNow.AddDays(-20) 
          },

          new Transaction
          {
            UserId = userId,
            CoinSymbol = "BTC",     // Bitcoin sattık
            Type = "sell",
            Amount = 0.1m,          // 0.1 BTC
            Price = 65000m,         // $65,000'den — kar ettik
            Date = DateTime.UtcNow.AddDays(-10)
          }

        };

    // Tüm işlemleri veritabanına ekle
    context.Transactions.AddRange(transactions); 
    await context.SaveChangesAsync();
    }
}