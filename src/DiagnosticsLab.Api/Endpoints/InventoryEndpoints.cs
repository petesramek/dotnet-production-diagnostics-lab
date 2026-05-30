using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/inventory");

        group.MapGet("/problem", async (string sku, FakeInventoryClient client, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Inventory.Problem");
            const int maxAttempts = 5;
            var attempts = 0;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                attempts++;

                try
                {
                    logger.LogInformation("Inventory attempt {Attempt} for SKU {Sku}", attempt, sku);
                    var stock = await client.GetAvailableStockAsync(sku, cancellationToken);
                    return Results.Ok(new InventoryResult(sku, stock, attempts, Resilient: false));
                }
                catch (InventoryUnavailableException exception)
                {
                    logger.LogWarning(exception, "Inventory attempt {Attempt} failed for SKU {Sku}; retrying immediately", attempt, sku);
                }
            }

            return Results.Json(
                new InventoryFailure(sku, attempts, "Inventory provider unavailable after immediate retries", Resilient: false),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        group.MapGet("/improved", async (string sku, FakeInventoryClient client, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Inventory.Improved");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            const int maxAttempts = 3;
            var attempts = 0;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                attempts++;

                try
                {
                    logger.LogInformation("Inventory attempt {Attempt} of {MaxAttempts} for SKU {Sku}", attempt, maxAttempts, sku);
                    var stock = await client.GetAvailableStockAsync(sku, timeout.Token);
                    return Results.Ok(new InventoryResult(sku, stock, attempts, Resilient: true));
                }
                catch (InventoryUnavailableException exception) when (attempt < maxAttempts)
                {
                    var delay = TimeSpan.FromMilliseconds(100 * attempt);
                    logger.LogWarning(exception, "Inventory attempt {Attempt} failed for SKU {Sku}; waiting {DelayMs} ms before retry", attempt, sku, delay.TotalMilliseconds);
                    await Task.Delay(delay, timeout.Token);
                }
                catch (InventoryUnavailableException exception)
                {
                    logger.LogError(exception, "Inventory provider unavailable for SKU {Sku} after {Attempts} controlled attempts", sku, attempts);
                }
            }

            return Results.Json(
                new InventoryFailure(sku, attempts, "Inventory provider unavailable after controlled retries", Resilient: true),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return endpoints;
    }

    private sealed record InventoryResult(string Sku, int Stock, int Attempts, bool Resilient);

    private sealed record InventoryFailure(string Sku, int Attempts, string Error, bool Resilient);
}
