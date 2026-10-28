using System.Text.Json;

namespace DiagnosticsLab.Api.Scenarios.Memory;

/// <summary>
/// Maps endpoints for LOH fragmentation scenarios.
/// </summary>
public static class LohFragmentationEndpoints {
    private const string Route = "/16-loh-fragmentation";
    private const int ItemPayloadSize = 2_000; // each item stays well below the 85 KB LOH threshold
    private const int DefaultItemCount = 200; // ~400 KB body → buffering it crosses the LOH threshold

    public static IEndpointRouteBuilder MapLohFragmentationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // The entire request body is buffered into memory before processing.
        // A MemoryStream grows by doubling, so a body larger than 85 KB ends up in
        // buffers that live on the Large Object Heap (LOH), which is only collected
        // with gen2 and can fragment under sustained load. The deserialized list
        // then holds every item in memory at once.
        group.MapPost("/problem", async (
            HttpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("LohFragmentation.Problem");

                logger.LogWarning("Buffering entire request body into memory");

                using var memory = new MemoryStream();

                // Simulation:
                // Copying the full request body represents scenarios such as:
                // - large JSON payloads from clients
                // - file uploads buffered in memory
                //
                // This forces allocation of large buffers on the LOH.
                await request.Body.CopyToAsync(memory, cancellationToken);

                memory.Position = 0;

                var result = await JsonSerializer.DeserializeAsync<List<LargeItem>>(
                    memory,
                    cancellationToken: cancellationToken);

                // Because full buffering is used:
                // - large allocations are created
                // - memory fragmentation may occur under sustained load
                return Results.Ok(new {
                    Items = result?.Count ?? 0,
                    Streamed = false,
                    LohAllocation = true
                });
            });

        // Mitigation:
        // Stream the request body directly instead of buffering it.
        // This avoids large intermediate allocations and reduces LOH pressure.
        group.MapPost("/mitigation", async (
            HttpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("LohFragmentation.Mitigation");

                logger.LogInformation("Streaming request body without full buffering");

                // Simulation:
                // Reading directly from the request stream represents:
                // - streaming JSON deserialization
                // - processing data as it arrives
                //
                // Framework note: JsonSerializer.DeserializeAsyncEnumerable<T> reads the body
                // incrementally and yields one item at a time, so neither the raw body nor the
                // full List<T> is ever held in memory. Each item is small (< 85 KB), so it stays
                // on the regular generational heap and is collected cheaply.
                var count = 0;

                await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<LargeItem>(
                    request.Body,
                    cancellationToken: cancellationToken)) {
                    if (item is not null) {
                        count++;
                    }
                }

                // Because streaming is used:
                // - no large contiguous buffer is allocated
                // - memory usage stays stable under load
                return Results.Ok(new {
                    Items = count,
                    Streamed = true,
                    LohAllocation = false
                });
            });

        // Helper (not part of scenario):
        // Generates a payload whose total size exceeds the LOH threshold (85 KB)
        // while every individual item stays small.
        //
        // Purpose:
        // - the problem endpoint buffers the whole body, so its buffers grow into the LOH
        // - the mitigation endpoint never holds more than one small item at a time
        // - a few hundred KB per call keeps the lab safe to run repeatedly
        //
        // How to use:
        // 1. Call this endpoint:
        //      GET /16-loh-fragmentation/generate?items=200
        //
        // 2. Take the response body (JSON array)
        //
        // 3. POST it to:
        //      /16-loh-fragmentation/problem
        //   or:
        //      /16-loh-fragmentation/mitigation
        //
        // 4. Observe memory behavior (e.g. GC, allocations)
        //
        // This keeps the lab self-contained without requiring external tools.
        group.MapGet("/generate", (int? items) => {
            var count = Math.Clamp(items ?? DefaultItemCount, 1, 1_000);
            var payload = new string('x', ItemPayloadSize);

            return Results.Ok(Enumerable.Range(1, count).Select(i => new LargeItem(i, payload)));
        });

        return endpoints;
    }

    private sealed record LargeItem(int Id, string Payload);
}