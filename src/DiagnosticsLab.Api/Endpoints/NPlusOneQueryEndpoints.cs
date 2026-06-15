using DiagnosticsLab.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for N+1 query scenarios.
/// </summary>
public static class NPlusOneQueryEndpoints {
    private const string Route = "/05-n-plus-1-data-access";

    public static IEndpointRouteBuilder MapNPlusOneQueryEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Multiple queries are executed per entity (N+1 problem).
        // This results in excessive database round-trips and poor performance.
        group.MapGet("/problem", async (
            int? take,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("NPlusOneQuery.Problem");

                var normalizedTake = NormalizeTake(take);

                logger.LogWarning("Executing N+1 queries for {Take} customers", normalizedTake);

                // First query: load customers
                var customers = await db.Customers
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .Take(normalizedTake)
                    .ToListAsync(cancellationToken);

                var result = new List<CustomerSummary>(customers.Count);

                foreach (var customer in customers) {
                    // Simulation:
                    // Each iteration triggers additional database queries.
                    //
                    // Real-world equivalent:
                    // - 1 query to load customers
                    // - N queries for order count
                    // - N queries for order totals
                    //
                    // Total: 1 + 2N queries → classic N+1 problem

                    var orderCount = await db.Orders
                        .AsNoTracking()
                        .CountAsync(o => o.CustomerId == customer.Id, cancellationToken);

                    var totalSpent = await db.Orders
                        .AsNoTracking()
                        .Where(o => o.CustomerId == customer.Id)
                        .Select(o => (double?)o.Total)
                        .SumAsync(cancellationToken) ?? 0d;

                    // Because queries are executed per customer:
                    // - each customer incurs additional round-trips
                    // - latency grows linearly with number of customers
                    result.Add(new CustomerSummary(
                        customer.Id,
                        customer.Name,
                        customer.Segment,
                        orderCount,
                        totalSpent));
                }

                return Results.Ok(result);
            });

        // Mitigation:
        // Fetch all required data in a single query using projection.
        group.MapGet("/mitigation", async (
            int? take,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("NPlusOneQuery.Mitigation");

                var normalizedTake = NormalizeTake(take);

                logger.LogInformation("Executing optimized single query for {Take} customers", normalizedTake);

                // Simulation:
                // This represents a single database query that retrieves:
                // - customers
                // - related aggregates
                //
                // Real-world equivalent:
                // A single SQL query with joins / subqueries

                var result = await db.Customers
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .Take(normalizedTake)
                    .Select(c => new CustomerSummary(
                        c.Id,
                        c.Name,
                        c.Segment,

                        db.Orders.Count(o => o.CustomerId == c.Id),

                        db.Orders
                            .Where(o => o.CustomerId == c.Id)
                            .Select(o => (double?)o.Total)
                            .Sum() ?? 0d))
                    .ToListAsync(cancellationToken);

                // Because everything is executed in one query:
                // - only one round-trip occurs
                // - performance scales with data size efficiently
                return Results.Ok(result);
            });

        return endpoints;
    }

    /// <summary>
    /// Normalizes the requested number of customers to a safe range.
    ///
    /// This prevents:
    /// - excessive query volume in the problem scenario
    /// - unrealistic inputs that could distort performance observations
    ///
    /// In real-world APIs, similar limits protect the system from:
    /// - large fan-out query patterns
    /// - excessive database load caused by user input
    /// </summary>
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
