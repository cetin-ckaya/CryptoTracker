# CryptoTracker 📈

Kullanıcıların kripto para portföylerini gerçek zamanlı olarak takip edebildiği, alım/satım işlemlerini kaydederek otomatik kar/zarar hesabı yapabildiği full-stack web uygulaması.

---

## 🚀 Özellikler

- 🔐 **JWT tabanlı kimlik doğrulama** — kayıt, giriş ve korumalı route yapısı
- 📊 **Portföy dashboard'u** — toplam değer, yatırım, kar/zarar özeti
- 💱 **Al/Sat işlemi yönetimi** — işlem ekleme, listeleme, silme
- 📈 **Gerçek zamanlı fiyatlar** — CoinGecko API ile 15.000+ coin desteği
- 🧮 **Otomatik hesaplama** — ortalama maliyet, kar/zarar yüzdesi
- 🔍 **Pagination & filtreleme** — coin sembolü ve işlem tipine göre filtreleme

---

## 🛠️ Teknolojiler

### Backend
| Teknoloji | Kullanım |
|-----------|----------|
| .NET 8 Web API | RESTful API geliştirme |
| ASP.NET Core Identity | Kullanıcı yönetimi |
| JWT Authentication | Token tabanlı kimlik doğrulama |
| Entity Framework Core | ORM ve veritabanı yönetimi |
| SQLite | Veritabanı |
| AutoMapper | Nesne dönüşümleri |
| Repository Pattern | Veri erişim katmanı |
| CoinGecko API | Anlık coin fiyatları |

### Frontend
| Teknoloji | Kullanım |
|-----------|----------|
| React (Vite) | UI geliştirme |
| React Router | Sayfa yönetimi |
| Axios | HTTP istekleri |

---

## 🏗️ Mimari

```
CryptoTracker/
├── Controllers/         # HTTP isteklerini karşılar
├── Services/            # İş mantığı katmanı
├── Repositories/        # Veri erişim katmanı
├── Models/              # Veritabanı modelleri
├── DTOs/                # Veri transfer nesneleri
├── Data/                # DbContext ve Seed data
└── Mappings/            # AutoMapper profilleri

cryptotracker-client/
├── src/
│   ├── pages/           # Login, Register, Dashboard, Transactions
│   └── services/        # Axios API servisi
```

---

## ⚙️ Kurulum

### Gereksinimler
- .NET 8 SDK
- Node.js 18+

### Backend

```bash
git clone https://github.com/cetin-ckaya/CryptoTracker.git
cd CryptoTracker
dotnet ef database update
dotnet run
```

API `http://localhost:5071` adresinde çalışır.

### Frontend

```bash
git clone https://github.com/cetin-ckaya/CryptoTracker-Frontend.git
cd cryptotracker-client
npm install
npm run dev
```

Uygulama `http://localhost:5173` adresinde çalışır.

---

## 📡 API Endpoint'leri

| Method | Endpoint | Açıklama | Auth |
|--------|----------|----------|------|
| POST | `/api/auth/register` | Kayıt ol | ❌ |
| POST | `/api/auth/login` | Giriş yap, JWT al | ❌ |
| GET | `/api/crypto/me` | Giriş yapmış kullanıcı | ✅ |
| GET | `/api/transaction` | İşlem listesi (pagination) | ✅ |
| POST | `/api/transaction` | Yeni işlem ekle | ✅ |
| DELETE | `/api/transaction/{id}` | İşlem sil | ✅ |
| GET | `/api/portfolio/summary` | Portföy özeti | ✅ |
| GET | `/api/portfolio/{symbol}` | Coin portföyü | ✅ |
| GET | `/api/coin/prices` | Anlık fiyatlar | ✅ |

---

## 🔗 Repolar

- **Backend:** [github.com/cetin-ckaya/CryptoTracker](https://github.com/cetin-ckaya/CryptoTracker)
- **Frontend:** [github.com/cetin-ckaya/CryptoTracker-Frontend](https://github.com/cetin-ckaya/CryptoTracker-Frontend)
