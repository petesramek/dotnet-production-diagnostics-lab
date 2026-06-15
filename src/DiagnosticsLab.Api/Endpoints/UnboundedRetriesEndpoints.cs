using DiagnosticsLab.Api.Services;
using DiagnosticsLab;

/// <summary>
/// Maps endpoints for unbounded retry scenarios.
/// </summary>
public static class UnboundedRetriesEndpoints {
    private const string Route = "/07-unbounded-retries";

    public static IEndpointRouteBuilder MapUnboundedRetriesEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Retries are executed immediately without delay or control.
        // This amplifies failures and can overload the dependency.
        group.MapGet("/problem", async (
            string sku,
            FakeInventoryClient client,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("UnboundedRetries.Problem");

                const int maxAttempts = 5;
                var attempts = 0;

                for (var attempt = 1; attempt <= maxAttempts; attempt++) {
                    attempts++;

                    try {
                        logger.LogWarning("Attempt {Attempt} for SKU {Sku}", attempt, sku);

                        // Simulation:
                        // FakeInventoryClient represents an external dependency (API/service).
                        //
                        // When it fails:
                        // - immediate retries are triggered
                        // - no delay allows system to recover
                        // - multiple callers retry simultaneously
                        //
                        // This creates a retry storm under load.
                        var stock = await client.GetAvailableStockAsync(sku, cancellationToken);

                        return Results.Ok(new InventoryResult(
                            sku,
                            stock,
                            attempts,
                            Resilient: false));
                    } catch (InventoryUnavailableException exception) {
                        // Immediate retry without delay
                        // → increases pressure on already failing service
                        logger.LogWarning(exception,
                            "Attempt {Attempt} failed for SKU {Sku}; retrying immediately",
                            attempt,
                            sku);
                    }
                }

                return Results.Json(
                    new InventoryFailure(
                        sku,
                        attempts,
                        "Inventory provider unavailable after immediate retries",
                        Resilient: false),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            });

        // Mitigation:
        // Apply controlled retries with limits and backoff delay.
        // This reduces pressure on dependencies and improves recovery.
        group.MapGet("/mitigation", async (
            string sku,
            FakeInventoryClient client,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("UnboundedRetries.Mitigation");

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));

                const int maxAttempts = 3;
                var attempts = 0;

                for (var attempt = 1; attempt <= maxAttempts; attempt++) {
                    attempts++;

                    try {
                        logger.LogInformation(
                            "Attempt {Attempt}/{MaxAttempts} for SKU {Sku}",
                            attempt,
                            maxAttempts,
                            sku);

                        // Simulation:
                        // Same external dependency call as in problem case.
                        //
                        // Difference:
                        // - retry count is limited
                        // - timeout bounds total execution
                        // - delay is introduced between attempts
                        var stock = await client.GetAvailableStockAsync(sku, timeout.Token);

                        return Results.Ok(new InventoryResult(
                            sku,
                            stock,
                            attempts,
                            Resilient: true));
                    } catch (InventoryUnavailableException exception) when (attempt < maxAttempts) {
                        var delay = TimeSpan.FromMilliseconds(100 * attempt);

                        logger.LogWarning(exception,
                            "Attempt {Attempt} failed for SKU {Sku}; waiting {DelayMs} ms before retry",
                            attempt,
                            sku,
                            delay.TotalMilliseconds);

                        // Simulation:
                        // Backoff delay gives the dependency time to recover.
                        //
                        // Real-world effect:
                        // - reduces retry frequency
                        // - prevents synchronized retry spikes
                        // - limits cascading failures
                        await Task.Delay(delay, timeout.Token);
                    } catch (InventoryUnavailableException exception) {
                        logger.LogError(exception,
                            "Dependency unavailable for SKU {Sku} after {Attempts} attempts",
                            sku,
                            attempts);
                    }
                }

                return Results.Json(
                    new InventoryFailure(
                        sku,
                        attempts,
                        "Inventory provider unavailable after controlled retries",
                        Resilient: true),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            });

        return endpoints;
    }

    private sealed record InventoryResult(string Sku, int Stock, int Attempts, bool Resilient);

    private sealed record InventoryFailure(string Sku, int Attempts, string Error, bool Resilient);
}

