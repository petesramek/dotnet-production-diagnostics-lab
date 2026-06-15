using System.Diagnostics;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for blocking vs asynchronous request handling scenarios.
/// </summary>
public static class BlockingRequestHandlingEndpoints {
    private const string Route = "/06-blocking-request-handling";

    public static IEndpointRouteBuilder MapBlockingRequestHandlingEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Blocking operations occupy ThreadPool threads and reduce system throughput under load.
        group.MapGet("/problem", (int? delayMs, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("BlockingRequestHandling.Problem");

            var delay = NormalizeDelay(delayMs);
            var stopwatch = Stopwatch.StartNew();

            logger.LogWarning("Blocking ThreadPool thread for {DelayMs} ms", delay);

            // Simulation:
            // Thread.Sleep blocks the current thread completely.
            //
            // Real-world equivalent:
            // - synchronous I/O calls
            // - blocking database operations
            // - CPU-bound work executed on request thread
            //
            // While the thread is blocked:
            // - it cannot process other requests
            // - ThreadPool may grow under pressure
            // - request latency increases under load
            Thread.Sleep(delay);

            stopwatch.Stop();

            return Results.Ok(new {
                DelayMs = delay,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Blocking = true
            });
        });

        // Mitigation:
        // Use asynchronous operations so threads are released while waiting.
        group.MapGet("/mitigation", async (int? delayMs, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("BlockingRequestHandling.Mitigation");

            var delay = NormalizeDelay(delayMs);
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Processing asynchronously for {DelayMs} ms", delay);

            // Simulation:
            // Task.Delay represents asynchronous waiting (non-blocking).
            //
            // Real-world equivalent:
            // - async database calls
            // - async HTTP calls
            //
            // While awaiting:
            // - thread is returned to ThreadPool
            // - other requests can be processed
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
    /// Normalizes delay to a safe range.
    ///
    /// Prevents:
    /// - extremely long blocking operations
    /// - unrealistic test inputs
    ///
    /// In real systems, similar limits protect thread pool stability.
    /// </summary>
    private static int NormalizeDelay(int? delayMs) {
        return Math.Clamp(delayMs ?? 500, 1, 5000);
    }
}