namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for missing cancellation and timeout handling scenarios.
/// </summary>
public static class CancellationTimeoutsEndpoints {
    private const string Route = "/02-cancellation-timeouts";

    public static IEndpointRouteBuilder MapCancellationTimeoutsEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Async work ignores CancellationToken and continues even after request is aborted.
        group.MapGet("/problem", async (ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("CancellationTimeouts.Problem");

            logger.LogWarning("Executing operation without cancellation support");

            // Simulation:
            // Represents async work such as:
            // - database queries (ToListAsync)
            // - HTTP calls (SendAsync)
            //
            // CancellationToken is NOT passed, so this work continues
            // even if the client disconnects.
            await Task.Delay(TimeSpan.FromSeconds(5));

            return Results.Ok(new {
                Status = "Completed",
                CancellationAware = false
            });
        });

        // Mitigation:
        // Pass CancellationToken so work stops immediately when request is aborted.
        group.MapGet("/mitigation", async (
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("CancellationTimeouts.Mitigation");

                logger.LogInformation("Executing operation with cancellation support");

                // Simulation:
                // Same async work as above, but CancellationToken is propagated.
                //
                // When request is aborted:
                // - operation is cancelled immediately
                // - remaining work is skipped
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                return Results.Ok(new {
                    Status = "Completed",
                    CancellationAware = true
                });
            });

        return endpoints;
    }
}