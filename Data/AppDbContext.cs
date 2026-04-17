using Microsoft.EntityFrameworkCore;
using CryptoTracker.Models;

namespace CryptoTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } //ne işe yarıyor

    public DbSet<User> Users { get; set; } // ne işe yarıyor
    public DbSet<Transaction> Transactions { get; set; } // ne işe yarıyor
}
