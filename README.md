# CryptoTracker 📈

Kullanıcıların kripto para portföylerini takip edebildiği, kar/zarar hesaplayabildiği full-stack bir web uygulaması.

## Teknolojiler

**Backend**
- .NET 8 Web API
- Entity Framework Core (SQLite)
- ASP.NET Identity
- JWT Authentication

**Frontend** *(yapım aşamasında)*
- React
- Axios

## Özellikler

- Kullanıcı kayıt ve giriş sistemi (JWT)
- Coin alım/satım işlemi ekleme
- Ortalama maliyet hesaplama
- Kar/zarar hesaplama
- Gerçek zamanlı coin fiyatları (CoinGecko API)
- Portföy dağılım grafiği

## Kurulum

```bash
# Projeyi klonla
git clone https://github.com/cetin-ckaya/CryptoTracker.git
cd CryptoTracker

# Veritabanını oluştur
dotnet ef database update

# Uygulamayı başlat
dotnet run
```

## API Endpointleri

| Method | URL | Açıklama |
|--------|-----|----------|
| POST | /api/auth/register | Kayıt ol |
| POST | /api/auth/login | Giriş yap, JWT token al |

## Geliştirme Süreci

Bu proje 20 günlük bir geliştirme planı ile yapılmaktadır.
