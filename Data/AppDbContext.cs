using Microsoft.EntityFrameworkCore;
using CryptoTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CryptoTracker.Data;

// IdentityDbContext → Identity'nin kendi tablolarını
// (Users, Roles, Claims vs.) otomatik oluşturan DbContext.
// Biz AppDbContext'i bundan miras alıyoruz.
public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Transaction> Transactions { get; set; } 
}
