using Microsoft.Extensions.Caching.Memory;

namespace DiagnosticsLab.Api.Scenarios.DependencyReliability;

/// <summary>
/// Maps endpoints for cache stampede scenarios.
/// </summary>
public static class CacheStampedeEndpoints {
    private const string Route = "/12-cache-stampede";
    private const string CacheKey = "hot-key";

    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static IEndpointRouteBuilder MapCacheStampedeEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Multiple concurrent requests recompute the same value when cache is empty.
        group.MapGet("/problem", async (IMemoryCache cache, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
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
                await Task.Delay(200, cancellationToken);

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
        //
        // Framework alternative:
        // HybridCache (Microsoft.Extensions.Caching.Hybrid, registered with services.AddHybridCache())
        // has stampede protection built in. Concurrent GetOrCreateAsync calls for the same key
        // share a single factory execution, so no hand-written semaphore or double-checked
        // locking is needed, and a per-key lock replaces the single global one used here:
        //
        //   var value = await hybridCache.GetOrCreateAsync(
        //       CacheKey,
        //       async token => await ComputeExpensiveValueAsync(token),
        //       cancellationToken: cancellationToken);
        group.MapGet("/mitigation", async (IMemoryCache cache, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
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
            // The token lets a caller that disconnects stop waiting for the lock.
            await Semaphore.WaitAsync(cancellationToken);

            try {
                // Double-check after acquiring lock
                if (!cache.TryGetValue(CacheKey, out cached)) {
                    logger.LogWarning("Cache miss (single recompute)");

                    // Simulation:
                    // Expensive operation executed only once,
                    // preventing duplicate work under concurrency.
                    await Task.Delay(200, cancellationToken);

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