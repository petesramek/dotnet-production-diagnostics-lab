using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for external dependency timeout scenarios.
/// </summary>
public static class ExternalDependencyReliabilityEndpoints {
    /// <summary>
    /// Adds external dependency reliability endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapExternalDependencyReliabilityEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/04-external-dependency-reliability");

        group.MapGet("/problem", async (string country, FakeShippingClient client, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("Shipping.Problem");

            logger.LogInformation("Requesting shipping rate for {Country} without timeout", country);

            var rate = await client.GetRateAsync(country, CancellationToken.None);

            return Results.Ok(new {
                Country = country,
                Rate = rate,
                Resilient = false
            });
        });

        group.MapGet("/improved", async (string country, FakeShippingClient client, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Shipping.Resilient");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            try {
                logger.LogInformation("Requesting shipping rate for {Country} with timeout", country);

                var rate = await client.GetRateAsync(country, timeout.Token);

                return Results.Ok(new {
                    Country = country,
                    Rate = rate,
                    Resilient = true
                });
            } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
                logger.LogWarning("Shipping provider timed out for {Country}", country);

                return Results.StatusCode(StatusCodes.Status504GatewayTimeout);
            }
        });

        return endpoints;
    }
}