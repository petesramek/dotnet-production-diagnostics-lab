namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for missing and improved cancellation scenarios.
/// </summary>
public static class ReportsEndpoints
{
    /// <summary>
    /// Adds report diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reports");

        group.MapGet("/slow", async (ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Reports.Slow");
            logger.LogInformation("Starting slow report without cancellation support");

            await Task.Delay(TimeSpan.FromSeconds(5));

            return Results.Ok(new { Status = "Completed", CancellationAware = false });
        });

        group.MapGet("/cancellable", async (ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Reports.Cancellable");
            logger.LogInformation("Starting cancellable report");

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            return Results.Ok(new { Status = "Completed", CancellationAware = true });
        });

        return endpoints;
    }
}
