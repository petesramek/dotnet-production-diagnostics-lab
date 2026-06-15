using System.Diagnostics;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for ThreadPool starvation scenarios.
/// </summary>
public static class ThreadPoolStarvationEndpoints {
    private const string Route = "/15-threadpool-starvation";

    public static IEndpointRouteBuilder MapThreadPoolStarvationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Blocking operations occupy ThreadPool threads.
        // Under load, this leads to ThreadPool starvation and increased response latency.
        group.MapGet("/problem", (int? delayMs, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("ThreadPoolStarvation.Problem");

            var delay = NormalizeDelay(delayMs);
            var stopwatch = Stopwatch.StartNew();

            logger.LogWarning("Blocking ThreadPool thread for {DelayMs} ms", delay);

            // Simulation:
            // Thread.Sleep blocks the current thread completely.
            //
            // Real-world equivalent:
            // - synchronous I/O calls
            // - lock contention
            // - CPU-bound work on request thread
            //
            // While the thread is blocked:
            // - it cannot process other requests
            // - ThreadPool must grow to compensate
            // - under load, requests get queued → latency spikes
            Thread.Sleep(delay);

            stopwatch.Stop();

            return Results.Ok(new {
                DelayMs = delay,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Blocking = true
            });
        });

        // Mitigation:
        // Use asynchronous, non-blocking operations so the thread can be returned
        // to the ThreadPool while waiting.
        group.MapGet("/mitigation", async (
            int? delayMs,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("ThreadPoolStarvation.Mitigation");

                var delay = NormalizeDelay(delayMs);
                var stopwatch = Stopwatch.StartNew();

                logger.LogInformation("Processing asynchronously for {DelayMs} ms", delay);

                // Simulation:
                // Task.Delay represents asynchronous waiting.
                //
                // Real-world equivalent:
                // - async I/O operations (database, HTTP calls)
                //
                // While awaiting:
                // - the thread is released back to the ThreadPool
                // - other requests can be processed
                // - system scales under load
                await Task.Delay(delay, cancellationToken);

                stopwatch.Stop();

                return Results.Ok(new {
                    DelayMs = delay,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Blocking = false
                });
            });

        return endpoints;
    }

    /// <summary>
    /// Normalizes delay input to a safe range.
    ///
    /// This prevents:
    /// - extremely long blocking operations in the problem case
    /// - unrealistic test values that distort behavior
    ///
    /// In real systems, similar constraints prevent abuse
    /// and protect ThreadPool stability.
    /// </summary>
    private static int NormalizeDelay(int? delayMs) {
        return Math.Clamp(delayMs ?? 500, 1, 5000);
    }
}