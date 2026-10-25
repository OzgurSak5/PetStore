using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;

namespace PetStore.Infrastructure.Data;

public class PetStoreDbContext : DbContext
{
    public PetStoreDbContext(DbContextOptions<PetStoreDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<PetPhoto> PetPhotos => Set<PetPhoto>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<Breed> Breeds => Set<Breed>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Favorite>().HasKey(f => new { f.UserId, f.PetId });

        modelBuilder.Entity<Pet>().Property(p => p.Status).HasConversion<string>();

        modelBuilder.Entity<Pet>().Property(p => p.Gender).HasConversion<string>();

        modelBuilder.Entity<Order>().Property(o => o.Status).HasConversion<string>();

        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();

        modelBuilder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.TokenHash);
        
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}