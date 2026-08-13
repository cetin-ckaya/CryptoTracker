using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CryptoTracker.Data;
using CryptoTracker.Models;
using Microsoft.Extensions.Options;
using CryptoTracker.Services;
using CryptoTracker.Repositories;
using CryptoTracker.Mappings;
using CryptoTracker.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Swagger servislerini ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controller'ları projeye tanıt
builder.Services.AddControllers();

// AuthService'i dependency injection'a kaydet
builder.Services.AddScoped<IAuthService, AuthService>();

// TransactionService'i dependency injection'a kaydet
builder.Services.AddScoped<ITransactionService, TransactionService>();

// PortfolioService'i dependency injection'a kaydet
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

// CoinService'i Scoped olarak kaydet
builder.Services.AddHttpClient<ICoinService, CoinService>(client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoTracker/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// TransactionRepository'yi dependency injection'a kaydet
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// SQLite veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity sistemi — kullanıcı kayıt/giriş yönetimi
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>();

// JWT ayarlarını appsettings.json'dan oku
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        ValidateLifetime = true
    };
});

// AutoMapper'ı projeye tanıt
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<MappingProfile>();
});

// CORS politikası — Vercel üzerindeki frontend'in istek atabilmesi için esnetildi
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Localhost ve Vercel dâhil tüm canlı isteklere izin ver
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Swagger'ı canlıda da test edebilmeniz için dışarı açıyoruz
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionMiddleware>();

// CORS middleware'i — tanımladığımız politikayı aktif et
app.UseCors("AllowReactApp");

// Kimlik doğrulama ve yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

// Uygulama başlarken Veritabanı Migration ve Seed Data çalıştırma
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(); 
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1. Tablolar yoksa otomatik oluştur (Migration uygula)
    await context.Database.MigrateAsync();

    // 2. Seed data'yı çalıştır
    await SeedData.InitializeAsync(userManager, context);

    // 3. CoinGecko cache'ini ön yükle
    var coinService = scope.ServiceProvider.GetRequiredService<ICoinService>();
    await coinService.GetCoinPriceAsync("BTC");
}   

app.MapControllers();

app.Run();