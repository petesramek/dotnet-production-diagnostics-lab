using DiagnosticsLab.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for slow and improved order data access scenarios.
/// </summary>
public static class OrdersEndpoints
{
    /// <summary>
    /// Adds order diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/orders");

        group.MapGet("/slow", async (int customerId, AppDbContext db, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Orders.Slow");
            logger.LogInformation("Loading all orders before filtering for customer {CustomerId}", customerId);

            var orders = await db.Orders.ToListAsync(cancellationToken);

            var result = orders
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAt)
                .Take(20)
                .Select(order => new OrderSummary(order.Id, order.CustomerId, order.CreatedAt, order.Total, order.Status));

            return Results.Ok(result);
        });

        group.MapGet("/improved", async (int customerId, AppDbContext db, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Orders.Improved");
            logger.LogInformation("Filtering orders in the database for customer {CustomerId}", customerId);

            var result = await db.Orders
                .AsNoTracking()
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => new OrderSummary(order.Id, order.CustomerId, order.CreatedAt, order.Total, order.Status))
                .Take(20)
                .ToListAsync(cancellationToken);

            return Results.Ok(result);
        });

        return endpoints;
    }

    private sealed record OrderSummary(int Id, int CustomerId, DateTimeOffset CreatedAt, decimal Total, string Status);
}
