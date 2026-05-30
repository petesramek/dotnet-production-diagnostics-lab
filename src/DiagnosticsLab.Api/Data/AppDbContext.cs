using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Data;

/// <summary>
/// Entity Framework Core database context used by the diagnostics lab.
/// </summary>
/// <param name="options">The database context options.</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the customers table.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets the orders table.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <inheritdoc />
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
        });
    }
}
