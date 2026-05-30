using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.CustomerId);
            entity.Property(order => order.Status).HasMaxLength(32);
            entity.Property(order => order.Total).HasPrecision(18, 2);
        });
    }
}
