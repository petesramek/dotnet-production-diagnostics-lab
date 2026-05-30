using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for external dependency timeout scenarios.
/// </summary>
public static class ShippingEndpoints
{
    /// <summary>
    /// Adds shipping diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapShippingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/shipping");

        group.MapGet("/problem", async (string country, FakeShippingClient client, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Shipping.Problem");
            logger.LogInformation("Requesting shipping rate for {Country} without timeout", country);

            var rate = await client.GetRateAsync(country, CancellationToken.None);
            return Results.Ok(new { Country = country, Rate = rate, Resilient = false });
        });

        group.MapGet("/resilient", async (string country, FakeShippingClient client, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Shipping.Resilient");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            try
            {
                logger.LogInformation("Requesting shipping rate for {Country} with timeout", country);
                var rate = await client.GetRateAsync(country, timeout.Token);
                return Results.Ok(new { Country = country, Rate = rate, Resilient = true });
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Shipping provider timed out for {Country}", country);
                return Results.StatusCode(StatusCodes.Status504GatewayTimeout);
            }
        });

        return endpoints;
    }
}
