using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CryptoTracker.Data;
using CryptoTracker.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Controller'ları projeye tanıt
builder.Services.AddControllers();

// SQLite veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity sistemi — kullanıcı kayıt/giriş yönetimi
// AddIdentity → User ve Role sınıflarını Identity'e tanıtıyoruz
// AddEntityFrameworkStores → Identity verilerini AppDbContext'e kaydet
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Şifre kuralları — geliştirme aşamasında kolaylaştırıyoruz
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

// JWT Authentication — gelen isteklerdeki token'ı doğrula
builder.Services.AddAuthentication(options =>
{
    // Varsayılan kimlik doğrulama yöntemi olarak JWT kullan
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options=>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Token'ın kim tarafından oluşturulduğunu doğrula
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        // Token'ın kime ait olduğunu doğrula
        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Token'ın imzasını doğrula — sahte token'ları engeller
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        // Token'ın süresinin dolup dolmadığını kontrol et
        ValidateLifetime = true
    };
});

// CORS politikası tanımla
// "AllowReactApp" → bu politikaya verdiğimiz isim, istediğimiz ismi verebiliriz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // React uygulamamızın çalışacağı adrese izin veriyoruz
        // İleride React'ı 3000 portunda başlatacağız
        policy.WithOrigins("http://localhost:3000")

        // Tüm HTTP metodlarına izin ver (GET, POST, PUT, DELETE)
        .AllowAnyMethod()

         // Tüm header'lara izin ver (Content-Type, Authorization gibi)
              // Authorization header'ı JWT token'ı taşıyacak
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// CORS middleware'i — tanımladığımız politikayı aktif et
// Bu satır olmazsa tarayıcı React'tan gelen istekleri engeller
app.UseCors("AllowReactApp");

// Kimlik doğrulama middleware'i — her istekte token kontrol edilir
app.UseAuthentication();

// Yetkilendirme middleware'i — [Authorize] attribute'u için gerekli
app.UseAuthorization();

app.MapControllers();

app.Run();