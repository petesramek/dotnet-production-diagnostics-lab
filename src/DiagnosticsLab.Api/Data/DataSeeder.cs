using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Data;

/// <summary>
/// Creates deterministic sample data for the diagnostics lab.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Ensures that the local database exists and contains sample customers and orders.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (!await db.Customers.AnyAsync())
        {
            var segments = new[] { "Startup", "SMB", "Enterprise" };

            var customers = Enumerable.Range(1, 250)
                .Select(index => new Customer
                {
                    Id = index,
                    Name = $"Customer {index:000}",
                    Segment = segments[index % segments.Length]
                })
                .ToArray();

            await db.Customers.AddRangeAsync(customers);
        }

        if (!await db.Orders.AnyAsync())
        {
            var random = new Random(42);
            var statuses = new[] { "Created", "Paid", "Shipped", "Cancelled" };

            var orders = Enumerable.Range(1, 10_000)
                .Select(index => new Order
                {
                    CustomerId = random.Next(1, 251),
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-index),
                    Total = Math.Round((decimal)(random.NextDouble() * 500), 2),
                    Status = statuses[random.Next(statuses.Length)]
                })
                .ToArray();

            await db.Orders.AddRangeAsync(orders);
        }

        await db.SaveChangesAsync();
    }
}
