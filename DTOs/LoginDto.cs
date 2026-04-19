namespace CryptoTracker.DTOs;


// Kullanıcı giriş yaparken göndereceği veriler
public class LoginDto
{
    // Email ile giriş yapacağız
    public string Email { get; set; } = string.Empty;

    // Şifre — Identity bunu hash ile karşılaştıracak
    public string Password { get; set; } = string.Empty;
}