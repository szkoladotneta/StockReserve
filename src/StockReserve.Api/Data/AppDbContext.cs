using Microsoft.EntityFrameworkCore;
using StockReserve.Api.Domain;

namespace StockReserve.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(p => p.Id);
            product.HasIndex(p => p.Sku).IsUnique();
            product.Property(p => p.Sku).HasMaxLength(32);
            product.Property(p => p.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Reservation>(reservation =>
        {
            reservation.HasKey(r => r.Id);
            reservation.HasIndex(r => new { r.Status, r.ExpiresAt });
            reservation.Property(r => r.CustomerEmail).HasMaxLength(200);
            reservation.Property(r => r.Status).HasConversion<string>().HasMaxLength(16);
            reservation
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
