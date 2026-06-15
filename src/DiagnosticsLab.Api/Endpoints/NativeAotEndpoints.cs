using System.Text.Json;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for Native AOT serialization scenarios.
/// </summary>
public static class NativeAotEndpoints {
    public static IEndpointRouteBuilder MapNativeAotEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/18-native-aot");

        // PROBLEM: reflection-based serialization
        group.MapPost("/problem", async (HttpRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("NativeAot.Problem");

            var result = await JsonSerializer.DeserializeAsync<SimplePayload>(
                request.Body);

            return Results.Ok(new {
                Name = result?.Name,
                Age = result?.Age,
                SourceGenerated = false
            });
        });

        // IMPROVED: source-generated serializer
        group.MapPost("/improved", async (HttpRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("NativeAot.Improved");

            var result = await JsonSerializer.DeserializeAsync(
                request.Body,
                NativeAotJsonContext.Default.SimplePayload);

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