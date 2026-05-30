namespace DiagnosticsLab.Api.Endpoints;

public static class ReportsEndpoints
{
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
