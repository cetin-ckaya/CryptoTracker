namespace CryptoTracker.DTOs;

// UserDto → User modelinin dışarıya gönderilen versiyonu
// PasswordHash burada YOK — güvenlik için asla dışarı çıkmamalı
public class UserDto
{
    // Kullanıcının ID'si — frontend'de hangi kullanıcı olduğunu anlamak için
    public string Id { get; set; } = string.Empty;

    // Kullanıcı adı — profil sayfasında göstereceğiz
    public string Username { get; set; } = string.Empty;

    // Email — kullanıcıya ait iletişim bilgisi
    public string Email { get; set; } = string.Empty;

    // Hesap oluşturulma tarihi — "üyelik tarihi" olarak göstereceğiz
    public DateTime CreatedAt { get; set; }
}