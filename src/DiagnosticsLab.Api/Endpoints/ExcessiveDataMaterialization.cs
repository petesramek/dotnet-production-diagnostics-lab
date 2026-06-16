using DiagnosticsLab.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for Excessive Data Materialization scenarios.
/// </summary>
public static class ExcessiveDataMaterializationEndpoints {
    private const string Route = "/01-excessive-data-materialization";

    public static IEndpointRouteBuilder MapExcessiveDataMaterializationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Loads the entire dataset into memory and filters afterward.
        // This leads to unnecessary data transfer and poor performance.
        group.MapGet("/problem", async (
            int customerId,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("ExcessiveDataMaterialization.Problem");

                logger.LogWarning("Loading all orders before filtering for customer {CustomerId}", customerId);

                // Simulation:
                // This represents fetching the full dataset from the database,
                // then applying filtering in memory.
                //
                // Real-world equivalent:
                // - dbContext.Orders.ToListAsync()
                // - then applying LINQ filtering in application
                //
                // This causes:
                // - unnecessary data transfer from database
                // - higher memory usage
                // - slower response times

                var orders = await db.Orders.ToListAsync(cancellationToken);

                // Filtering happens AFTER data is already loaded into memory
                var result = orders
                    .Where(order => order.CustomerId == customerId)
                    .OrderByDescending(order => order.CreatedAtUtc)
                    .Take(20)
                    .Select(order => new OrderSummary(
                        order.Id,
                        order.CustomerId,
                        order.CreatedAtUtc,
                        order.Total,
                        order.Status));

                return Results.Ok(result);
            });

        // Mitigation:
        // Push filtering and projection to the database,
        // ensuring only required data is fetched.
        group.MapGet("/mitigation", async (
            int customerId,
            AppDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("ExcessiveDataMaterialization.Mitigation");

                logger.LogInformation("Filtering orders in database for customer {CustomerId}", customerId);

                // Simulation:
                // This represents a properly constructed database query where:
                // - filtering is executed in SQL
                // - only necessary rows are returned
                //
                // Real-world equivalent:
                // - SELECT ... WHERE CustomerId = ...
                // - executed directly by the database engine
                //
                // This reduces:
                // - data transfer
                // - memory usage
                // - query execution time

                var result = await db.Orders
                    .AsNoTracking()
                    .Where(order => order.CustomerId == customerId)
                    .OrderByDescending(order => order.CreatedAtUtc)
                    .Select(order => new OrderSummary(
                        order.Id,
                        order.CustomerId,
                        order.CreatedAtUtc,
                        order.Total,
                        order.Status))
                    .Take(20)
                    .ToListAsync(cancellationToken);

                return Results.Ok(result);
            });

        return endpoints;
    }

    private sealed record OrderSummary(
        int Id,
        int CustomerId,
        DateTime CreatedAtUtc,
        double Total,
        string Status);
}