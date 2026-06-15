using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for external dependency reliability scenarios.
/// </summary>
public static class ExternalDependencyReliabilityEndpoints {
    private const string Route = "/04-external-dependency-reliability";

    public static IEndpointRouteBuilder MapExternalDependencyReliabilityEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // External dependency is called without timeout and may block indefinitely.
        group.MapGet("/problem", async (string country, FakeShippingClient client, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("ExternalDependencyReliability.Problem");

            logger.LogWarning("Calling external dependency without timeout for {Country}", country);

            // Simulation:
            // Represents an external system (e.g. HTTP API) that we do not control.
            // Without timeout:
            // - call may hang indefinitely
            // - threads and connections remain occupied
            var rate = await client.GetRateAsync(country, CancellationToken.None);

            return Results.Ok(new {
                Country = country,
                Rate = rate,
                Resilient = false
            });
        });

        // Mitigation:
        // Apply timeout and cancellation to prevent long-running dependency calls.
        group.MapGet("/mitigation", async (
            string country,
            FakeShippingClient client,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("ExternalDependencyReliability.Mitigation");

                // Simulation:
                // Same external dependency, but execution is bounded by timeout.
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));

                try {
                    logger.LogInformation("Calling external dependency with timeout for {Country}", country);

                    var rate = await client.GetRateAsync(country, timeout.Token);

                    return Results.Ok(new {
                        Country = country,
                        Rate = rate,
                        Resilient = true
                    });
                } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
                    logger.LogWarning("External dependency timed out for {Country}", country);

                    return Results.StatusCode(StatusCodes.Status504GatewayTimeout);
                }
            });

        return endpoints;
    }
}