using System.Diagnostics;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for blocking and asynchronous request handling scenarios.
/// </summary>
public static class BlockingRequestHandlingEndpoints {
    /// <summary>
    /// Adds blocking request handling diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapBlockingRequestHandlingEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/06-blocking-request-handling");

        group.MapGet("/problem", (int? delayMs, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("Blocking.Problem");

            var normalizedDelay = NormalizeDelay(delayMs);

            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Blocking request thread for {DelayMs} ms", normalizedDelay);

            Thread.Sleep(normalizedDelay);

            stopwatch.Stop();

            return Results.Ok(new BlockingResult(
                "Completed",
                normalizedDelay,
                stopwatch.ElapsedMilliseconds,
                Blocking: true));
        });

        group.MapGet("/improved", async (int? delayMs, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Blocking.Improved");

            var normalizedDelay = NormalizeDelay(delayMs);

            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Waiting asynchronously for {DelayMs} ms", normalizedDelay);

            await Task.Delay(normalizedDelay, cancellationToken);

            stopwatch.Stop();

            return Results.Ok(new BlockingResult(
                "Completed",
                normalizedDelay,
                stopwatch.ElapsedMilliseconds,
                Blocking: false));
        });

        return endpoints;
    }

    private static int NormalizeDelay(int? delayMs) {
        return Math.Clamp(delayMs ?? 500, 1, 5_000);
    }

    private sealed record BlockingResult(
        string Status,
        int RequestedDelayMs,
        long ElapsedMs,
        bool Blocking);
}
