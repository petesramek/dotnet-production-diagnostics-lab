using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Orders.AnyAsync())
        {
            return;
        }

        var random = new Random(42);
        var statuses = new[] { "Created", "Paid", "Shipped", "Cancelled" };

        var orders = Enumerable.Range(1, 10_000)
            .Select(index => new Order
            {
                CustomerId = random.Next(1, 250),
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-index),
                Total = Math.Round((decimal)(random.NextDouble() * 500), 2),
                Status = statuses[random.Next(statuses.Length)]
            })
            .ToArray();

        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();
    }
}
