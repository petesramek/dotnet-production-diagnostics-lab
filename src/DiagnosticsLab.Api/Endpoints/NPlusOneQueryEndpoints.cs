using DiagnosticsLab.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for N+1 data access scenarios.
/// </summary>
public static class NPlusOneQueryEndpoints {
    /// <summary>
    /// Adds N+1 data access diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapNPlusOneQueryEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/05-n-plus-1-data-access");

        group.MapGet("/problem", async (int? take, AppDbContext db, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Customers.Problem");

            var normalizedTake = NormalizeTake(take);

            logger.LogInformation(
                "Loading {Take} customers and querying order aggregates one customer at a time",
                normalizedTake);

            var customers = await db.Customers
                .AsNoTracking()
                .OrderBy(customer => customer.Id)
                .Take(normalizedTake)
                .ToListAsync(cancellationToken);

            var result = new List<CustomerSummary>(customers.Count);

            foreach (var customer in customers) {
                var orderCount = await db.Orders
                    .AsNoTracking()
                    .CountAsync(order => order.CustomerId == customer.Id, cancellationToken);

                var totalSpent = await db.Orders
                    .AsNoTracking()
                    .Where(order => order.CustomerId == customer.Id)
                    .Select(order => (double?)order.Total)
                    .SumAsync(cancellationToken) ?? 0d;

                result.Add(new CustomerSummary(
                    customer.Id,
                    customer.Name,
                    customer.Segment,
                    orderCount,
                    totalSpent));
            }

            return Results.Ok(result);
        });

        group.MapGet("/improved", async (int? take, AppDbContext db, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Customers.Improved");

            var normalizedTake = NormalizeTake(take);

            logger.LogInformation(
                "Loading {Take} customers with order aggregates in one projected query",
                normalizedTake);

            var result = await db.Customers
                .AsNoTracking()
                .OrderBy(customer => customer.Id)
                .Take(normalizedTake)
                .Select(customer => new CustomerSummary(
                    customer.Id,
                    customer.Name,
                    customer.Segment,
                    db.Orders.Count(order => order.CustomerId == customer.Id),
                    db.Orders
                        .Where(order => order.CustomerId == customer.Id)
                        .Select(order => (double?)order.Total)
                        .Sum() ?? 0d))
                .ToListAsync(cancellationToken);

            return Results.Ok(result);
        });

        return endpoints;
    }

    private static int NormalizeTake(int? take) {
        return Math.Clamp(take ?? 25, 1, 100);
    }

    private sealed record CustomerSummary(
        int Id,
        string Name,
        string Segment,
        int OrderCount,
        double TotalSpent);
}
