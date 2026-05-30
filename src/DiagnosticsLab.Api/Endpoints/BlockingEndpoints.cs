using System.Diagnostics;

namespace DiagnosticsLab.Api.Endpoints;

public static class BlockingEndpoints
{
    public static IEndpointRouteBuilder MapBlockingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/blocking");

        group.MapGet("/problem", (int? delayMs, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Blocking.Problem");
            var normalizedDelay = NormalizeDelay(delayMs);
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Blocking request thread for {DelayMs} ms", normalizedDelay);

            Thread.Sleep(normalizedDelay);

            stopwatch.Stop();
            return Results.Ok(new BlockingResult("Completed", normalizedDelay, stopwatch.ElapsedMilliseconds, Blocking: true));
        });

        group.MapGet("/improved", async (int? delayMs, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Blocking.Improved");
            var normalizedDelay = NormalizeDelay(delayMs);
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Waiting asynchronously for {DelayMs} ms", normalizedDelay);

            await Task.Delay(normalizedDelay, cancellationToken);

            stopwatch.Stop();
            return Results.Ok(new BlockingResult("Completed", normalizedDelay, stopwatch.ElapsedMilliseconds, Blocking: false));
        });

        return endpoints;
    }

    private static int NormalizeDelay(int? delayMs)
    {
        return Math.Clamp(delayMs ?? 500, 1, 5_000);
    }

    private sealed record BlockingResult(string Status, int RequestedDelayMs, long ElapsedMs, bool Blocking);
}
