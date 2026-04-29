using AutoMapper;
using CryptoTracker.DTOs;
using CryptoTracker.Models;
using Transaction = CryptoTracker.Models.Transaction;

namespace CryptoTracker.Mappings;

// MappingProfile → AutoMapper'a hangi sınıfın hangi sınıfa dönüşeceğini söylüyoruz
// Profile → AutoMapper'ın base sınıfı, bunu miras alıyoruz

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Transaction → TransactionDto dönüşümünü tanımla
        // CreateMap → "Transaction'ı TransactionDto'ya çevir" demek
        // AutoMapper aynı isimli alanları otomatik eşleştirir
        // Id, CoinSymbol, Type, Amount, Price, Date — hepsi aynı isimde olduğu için otomatik gider
        CreateMap<Transaction, TransactionDto>();

        // User → UserDto dönüşümünü tanımla
        // PasswordHash UserDto'da olmadığı için AutoMapper onu otomatik atlar
        // Id alanını açıkça eşleştiriyoruz çünkü AutoMapper karıştırıyor
       CreateMap<User, UserDto>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
        .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
        .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}