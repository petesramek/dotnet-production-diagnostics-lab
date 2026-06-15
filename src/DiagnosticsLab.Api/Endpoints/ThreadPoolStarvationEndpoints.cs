using System.Diagnostics;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for ThreadPool starvation scenarios.
/// </summary>
public static class ThreadPoolStarvationEndpoints {
    /// <summary>
    /// Adds ThreadPool starvation diagnostics endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapThreadPoolStarvationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/15-threadpool-starvation");

        // PROBLEM: blocking thread
        group.MapGet("/problem", (int? delayMs, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("ThreadPoolStarvation.Problem");

            var delay = NormalizeDelay(delayMs);

            var sw = Stopwatch.StartNew();

            logger.LogInformation("Blocking thread for {DelayMs} ms", delay);

            Thread.Sleep(delay); // blocks thread

            sw.Stop();

            return Results.Ok(new {
                DelayMs = delay,
                ElapsedMs = sw.ElapsedMilliseconds,
                Blocking = true
            });
        });

        // IMPROVED: async, non-blocking
        group.MapGet("/improved", async (int? delayMs, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("ThreadPoolStarvation.Improved");

            var delay = NormalizeDelay(delayMs);

            var sw = Stopwatch.StartNew();

            logger.LogInformation("Processing asynchronously for {DelayMs} ms", delay);

            await Task.Delay(delay, cancellationToken); // does not block thread

            sw.Stop();

            return Results.Ok(new {
                DelayMs = delay,
                ElapsedMs = sw.ElapsedMilliseconds,
                Blocking = false
            });
        });

        return endpoints;
    }

    private static int NormalizeDelay(int? delayMs) {
        return Math.Clamp(delayMs ?? 500, 1, 5000);
    }
}
