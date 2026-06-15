using Microsoft.Extensions.Caching.Memory;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for cache stampede scenarios.
/// </summary>
public static class CacheStampedeEndpoints {
    private const string Route = "/17-cache-stampede";
    private const string CacheKey = "hot-key";

    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static IEndpointRouteBuilder MapCacheStampedeEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Multiple concurrent requests recompute the same value when cache is empty.
        group.MapGet("/problem", async (IMemoryCache cache, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("CacheStampede.Problem");

            // Cache miss means value is not present.
            // Under concurrency, MANY requests hit this condition at the same time.
            if (!cache.TryGetValue(CacheKey, out string? value)) {
                logger.LogWarning("Cache miss → recomputing value");

                // Simulation:
                // Represents expensive operation (e.g. database / API call).
                //
                // Because there is no coordination:
                // - every request entering here recomputes the same value
                // - N concurrent requests → N identical expensive operations
                // - load is amplified dramatically (retry storm–like behavior)
                await Task.Delay(200);

                value = $"computed-{DateTime.UtcNow.Ticks}";
                cache.Set(CacheKey, value, TimeSpan.FromSeconds(5));
            }

            return Results.Ok(new {
                Value = value,
                Cached = true,
                Coordinated = false
            });
        });

        // Mitigation:
        // Ensure only one request recomputes the value.
        group.MapGet("/mitigation", async (IMemoryCache cache, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("CacheStampede.Mitigation");

            // Simulation:
            // Most requests should hit the cache and return immediately.
            if (cache.TryGetValue(CacheKey, out string? cached)) {
                return Results.Ok(new {
                    Value = cached,
                    Cached = true,
                    Coordinated = true
                });
            }

            // Only one request is allowed to recompute at a time.
            await Semaphore.WaitAsync();

            try {
                // Double-check after acquiring lock
                if (!cache.TryGetValue(CacheKey, out cached)) {
                    logger.LogWarning("Cache miss (single recompute)");

                    // Simulation:
                    // Expensive operation executed only once,
                    // preventing duplicate work under concurrency.
                    await Task.Delay(200);

                    cached = $"computed-{DateTime.UtcNow.Ticks}";
                    cache.Set(CacheKey, cached, TimeSpan.FromSeconds(5));
                }
            } finally {
                Semaphore.Release();
            }

            return Results.Ok(new {
                Value = cached,
                Cached = true,
                Coordinated = true
            });
        });

        return endpoints;
    }
}