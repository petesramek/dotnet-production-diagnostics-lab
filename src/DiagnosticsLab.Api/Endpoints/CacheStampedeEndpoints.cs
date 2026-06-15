using Microsoft.Extensions.Caching.Memory;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for cache stampede scenarios.
/// </summary>
public static class CacheStampedeEndpoints {
    private const string CacheKey = "hot-key";
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static IEndpointRouteBuilder MapCacheStampedeEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/17-cache-stampede");

        // PROBLEM: no coordination between concurrent callers
        group.MapGet("/problem", async (IMemoryCache cache, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("CacheStampede.Problem");

            if (!cache.TryGetValue(CacheKey, out string? value)) {
                logger.LogWarning("Cache miss → recomputing value");

                await Task.Delay(200); // simulate expensive work

                value = $"computed-{DateTime.UtcNow.Ticks}";

                cache.Set(CacheKey, value, TimeSpan.FromSeconds(5));
            }

            return Results.Ok(new {
                Value = value,
                Cached = true,
                Coordinated = false
            });
        });

        // IMPROVED: coordination using SemaphoreSlim
        group.MapGet("/improved", async (IMemoryCache cache, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("CacheStampede.Improved");

            if (cache.TryGetValue(CacheKey, out string? cached)) {
                return Results.Ok(new {
                    Value = cached,
                    Cached = true,
                    Coordinated = true
                });
            }

            await Semaphore.WaitAsync();

            try {
                // double-check (important)
                if (!cache.TryGetValue(CacheKey, out cached)) {
                    logger.LogWarning("Cache miss (single recompute)");

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
