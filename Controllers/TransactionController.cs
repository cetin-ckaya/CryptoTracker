using System.Security.Claims;
using CryptoTracker.DTOs;
using CryptoTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTracker.Controllers;

// TransactionController → coin alım/satım işlemlerini yönetir
// Tüm endpoint'ler [Authorize] gerektirir — giriş yapmadan erişilemez

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    // ITransactionService → iş mantığı için
    private readonly ITransactionService _transactionservice;

    public TransactionController(ITransactionService transactionservice)
    {
        _transactionservice = transactionservice;
    }

    // Token içinden giriş yapmış kullanıcının ID'sini al
    // Her endpoint'te kullandığımız için yardımcı metod yaptık
    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // GET api/transaction — kullanıcının tüm işlemlerini listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        //Tokendan kullanıcı ID si al
        var userId = GetUserId();

        //ID yoksa token geçersiz
        if(userId == null ) return Unauthorized();

        //Service den işlemleri al
        var transaction = await _transactionservice.GetUserTransactionAsync(userId);

        //200 Ok ile işlem listesi döndürülür
        return Ok(transaction);
    }

    //POST api ile yeni işlem ekle
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateTransactionDto dto)
    {
        //Tokendan kullanıcı Id si al
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        //Service e ekle - userId ve dto gönder
        var transaction = await _transactionservice.AddTransactionAsync(userId,dto);

        //201 Created döndür - yeni kaynak oluşturuldu
        //CreatedAction = oluşturulan kaynağın url sini header a ekler
        return CreatedAtAction(nameof(GetAll),transaction);
    }

    //Delete api ile işlem sil
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        //Tokendan kullanıcı Id si al
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        //Service e silme isteği gönder
        var result = await _transactionservice.DeleteTransactionAsync(id,userId);

        //İşlem bulunamadıysa 404 döndür
        if(!result) return NotFound(new {message = "İşlem Bulunamadı"});

        // Başarıyla silindiyse 204 No Content döndür
        // 204 → başarılı ama döndürülecek içerik yok
        return NoContent();
    }

}