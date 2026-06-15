using System.Text.Json;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for Large Object Heap (LOH) fragmentation scenarios.
/// </summary>
public static class LohFragmentationEndpoints {
    private const int LargePayloadSize = 200_000; // > 85 KB → LOH

    public static IEndpointRouteBuilder MapLohFragmentationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/16-loh-fragmentation");

        // PROBLEM: full buffering + large allocation
        group.MapPost("/problem", async (HttpRequest request, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("LohFragmentation.Problem");

            logger.LogInformation("Reading entire JSON payload into memory");

            using var memory = new MemoryStream();

            await request.Body.CopyToAsync(memory, cancellationToken);

            memory.Position = 0;

            var result = await JsonSerializer.DeserializeAsync<List<LargeItem>>(
                memory,
                cancellationToken: cancellationToken);

            return Results.Ok(new {
                Items = result?.Count ?? 0,
                Streamed = false,
                LohAllocation = true
            });
        });

        // IMPROVED: streaming deserialization
        group.MapPost("/improved", async (HttpRequest request, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("LohFragmentation.Improved");

            logger.LogInformation("Streaming JSON payload using System.Text.Json");

            var result = await JsonSerializer.DeserializeAsync<List<LargeItem>>(
                request.Body,
                cancellationToken: cancellationToken);

            return Results.Ok(new {
                Items = result?.Count ?? 0,
                Streamed = true,
                LohAllocation = false
            });
        });

        // helper endpoint to generate large payload
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