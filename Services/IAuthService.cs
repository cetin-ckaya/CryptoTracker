using CryptoTracker.DTOs;

namespace CryptoTracker.Services;

// Interface → AuthService'in dışarıya sunduğu metodların sözleşmesi
// Controller sadece bu interface'i bilir, içeride nasıl çalıştığını bilmez
// Bu sayede ileride AuthService'i değiştirirsek Controller etkilenmez
public interface IAuthService
{
    // Kayıt metodu — başarılıysa true, değilse hata mesajı döner
    // Task → bu metod asenkron çalışır, veritabanı işlemi beklememizi gerektirir
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);

    // Giriş metodu — başarılıysa JWT token döner, değilse null
    Task<string?> LoginAsync(LoginDto dto);
}