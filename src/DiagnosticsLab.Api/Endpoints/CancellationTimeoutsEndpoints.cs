namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for missing and improved cancellation scenarios.
/// </summary>
public static class CancellationTimeoutsEndpoints {
    /// <summary>
    /// Adds cancellation diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapCancellationTimeoutsEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/02-cancellation-timeouts");

        group.MapGet("/problem", async (ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("Reports.Slow");

            logger.LogInformation("Starting slow report without cancellation support");

            await Task.Delay(TimeSpan.FromSeconds(5));

            return Results.Ok(new { Status = "Completed", CancellationAware = false });
        });

        group.MapGet("/improved", async (ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Reports.Cancellable");

            logger.LogInformation("Starting cancellable report");

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            return Results.Ok(new { Status = "Completed", CancellationAware = true });
        });

        return endpoints;
    }
}