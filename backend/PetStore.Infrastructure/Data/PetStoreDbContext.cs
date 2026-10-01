using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;

namespace PetStore.Infrastructure.Data;

public class PetStoreDbContext : DbContext
{
    public PetStoreDbContext(DbContextOptions<PetStoreDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<PetPhoto> PetPhotos { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Species> Species { get; set; } = null!;
    public DbSet<Breed> Breeds { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Favorite>()
        .HasKey(f => new { f.UserId, f.PetId });

        modelBuilder.Entity<Pet>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Pet>()
            .Property(p => p.Gender)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}