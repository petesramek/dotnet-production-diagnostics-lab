using System.Text.Json;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for LOH fragmentation scenarios.
/// </summary>
public static class LohFragmentationEndpoints {
    private const string Route = "/16-loh-fragmentation";
    private const int LargePayloadSize = 200_000; // > 85 KB → LOH allocation

    public static IEndpointRouteBuilder MapLohFragmentationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // The entire request body is buffered into memory before processing.
        // Large allocations (≥ 85 KB) go to the Large Object Heap (LOH),
        // which can lead to fragmentation and increased GC pressure.
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
                // This avoids allocating a large contiguous buffer.
                var result = await JsonSerializer.DeserializeAsync<List<LargeItem>>(
                    request.Body,
                    cancellationToken: cancellationToken);

                // Because streaming is used:
                // - large temporary allocations are avoided
                // - memory usage remains stable under load
                return Results.Ok(new {
                    Items = result?.Count ?? 0,
                    Streamed = true,
                    LohAllocation = false
                });
            });

        // Helper (not part of scenario):
        // Generates a large payload to trigger LOH allocations.
        //
        // Purpose:
        // - LOH issues appear only with large objects (> 85 KB)
        // - this endpoint provides a deterministic way to create such payload
        //
        // How to use:
        // 1. Call this endpoint:
        //      GET /16-loh-fragmentation/generate
        //
        // 2. Take the response body (large JSON payload)
        //
        // 3. POST it to:
        //      /16-loh-fragmentation/problem
        //   or:
        //      /16-loh-fragmentation/mitigation
        //
        // 4. Observe memory behavior (e.g. GC, allocations)
        //
        // This keeps the lab self-contained without requiring external tools.
        group.MapGet("/generate", () => {
            var items = Enumerable.Range(1, 1000)
                .Select(i => new LargeItem(
                    i,
                    new string('x', LargePayloadSize)))
                .ToList();

            return Results.Ok(items);
        });

        return endpoints;
    }

    private sealed record LargeItem(int Id, string Payload);
}