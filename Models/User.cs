using Microsoft.AspNetCore.Identity;

namespace CryptoTracker.Models;


// IdentityUser → ASP.NET Identity'nin hazır kullanıcı sınıfı.
// Içinde zaten Username, Email, PasswordHash var.
// Biz bunu miras alarak kendi özel alanlarımızı ekliyoruz.
public class User : IdentityUser
{
     // Kullanıcının hesabı ne zaman oluşturuldu
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}