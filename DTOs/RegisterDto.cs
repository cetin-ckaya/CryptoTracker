namespace CryptoTracker.DTOs;

//Kullanıcı kayıt olurken göndereceği veriler
public class RegisterDto
{
    // Kullanıcı adı — benzersiz olmalı
    public string Username { get; set; } = string.Empty;

    // Email adresi — benzersiz olmalı
    public string Email { get; set; } = string.Empty;

    // Şifre — Identity otomatik hash'leyecek, düz metin saklanmayacak
    public string Password { get; set; } = string.Empty;
}