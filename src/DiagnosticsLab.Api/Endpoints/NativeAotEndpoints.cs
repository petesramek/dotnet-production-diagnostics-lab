using DiagnosticsLab.Api;
using System.Text.Json;

/// <summary>
/// Maps endpoints for Native AOT serialization scenarios.
/// </summary>
public static class NativeAotEndpoints {
    private const string Route = "/18-native-aot";

    public static IEndpointRouteBuilder MapNativeAotEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Reflection-based JSON serialization depends on runtime type metadata.
        // Under Native AOT, unused metadata can be trimmed,
        // causing serialization to fail at runtime.
        group.MapPost("/problem", async (HttpRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("NativeAot.Problem");

            logger.LogWarning("Using reflection-based JSON serialization");

            // Simulation:
            // Default JsonSerializer.DeserializeAsync relies on reflection
            // to discover type metadata at runtime.
            //
            // In Native AOT builds:
            // - reflection metadata may be trimmed
            // - serializer may fail with missing metadata errors
            //
            // This represents typical usage without source generation.
            var result = await JsonSerializer.DeserializeAsync<SimplePayload>(
                request.Body);

            // Because reflection is used:
            // - behavior depends on runtime metadata
            // - AOT trimming can break this code
            return Results.Ok(new {
                Name = result?.Name,
                Age = result?.Age,
                SourceGenerated = false
            });
        });

        // Mitigation:
        // Use source-generated serializers to ensure metadata is preserved
        // at compile time and compatible with Native AOT.
        group.MapPost("/mitigation", async (HttpRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("NativeAot.Mitigation");

            logger.LogInformation("Using source-generated JSON serialization");

            // Simulation:
            // This uses a JsonSerializerContext generated at compile time.
            //
            // Instead of relying on reflection:
            // - metadata is generated ahead-of-time
            // - no runtime discovery is required
            //
            // This is the recommended approach for:
            // - Native AOT
            // - high-performance serialization paths
            var result = await JsonSerializer.DeserializeAsync(
                request.Body,
                NativeAotJsonContext.Default.SimplePayload);

            // Because source generation is used:
            // - serialization works under trimming
            // - behavior is deterministic and safe for AOT
            return Results.Ok(new {
                Name = result?.Name,
                Age = result?.Age,
                SourceGenerated = true
            });
        });

        return endpoints;
    }

    public sealed record SimplePayload(string Name, int Age);
}