using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(128);
            entity.Property(customer => customer.Segment).HasMaxLength(32);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.CustomerId);
            entity.Property(order => order.Status).HasMaxLength(32);
            entity.Property(order => order.Total).HasPrecision(18, 2);
        });
    }
}
