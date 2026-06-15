using System.Security.Cryptography;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for request body memory pressure scenarios.
/// </summary>
public static class RequestBodyMemoryPressureEndpoints {
    private const string Route = "/13-request-body-memory-pressure";
    private const long MaxUploadBytes = 5 * 1024 * 1024;

    public static IEndpointRouteBuilder MapRequestBodyMemoryPressureEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // The entire request body is buffered into memory before processing.
        // Large uploads can cause excessive memory usage and GC pressure.
        group.MapPost("/problem", async (
            HttpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("RequestBodyMemoryPressure.Problem");

                logger.LogWarning("Buffering entire request body in memory");

                using var memory = new MemoryStream();

                // Simulation:
                // Copying the entire request body represents scenarios such as:
                // - file uploads
                // - large JSON payloads from clients
                //
                // In real applications, this pattern loads the full payload into memory
                // before processing, which can:
                // - create large allocations
                // - increase GC pressure
                // - reduce scalability under load
                await request.Body.CopyToAsync(memory, cancellationToken);

                return Results.Ok(new UploadResult(
                    memory.Length,
                    Streamed: false,
                    Limited: false));
            });

        // Mitigation:
        // Process the request body incrementally instead of buffering it entirely.
        // This keeps memory usage stable and allows controlled handling of large inputs.
        group.MapPost("/mitigation", async (
            HttpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("RequestBodyMemoryPressure.Mitigation");

                logger.LogInformation("Streaming request body with size limit");

                var buffer = new byte[64 * 1024];
                long totalBytes = 0;

                using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

                while (true) {
                    // Simulation:
                    // Reading the request body in chunks represents streaming processing,
                    // where data is handled incrementally instead of loaded entirely.
                    var read = await request.Body.ReadAsync(
                        buffer.AsMemory(0, buffer.Length),
                        cancellationToken);

                    if (read == 0) {
                        break;
                    }

                    totalBytes += read;

                    // Enforcing size limits prevents unbounded memory and resource usage
                    if (totalBytes > MaxUploadBytes) {
                        logger.LogWarning("Upload exceeded maximum allowed size");

                        return Results.BadRequest(new {
                            Error = "Upload is too large",
                            MaxUploadBytes
                        });
                    }

                    // Processing chunk-by-chunk avoids large intermediate allocations
                    hasher.AppendData(buffer, 0, read);
                }

                var hash = Convert.ToHexString(hasher.GetHashAndReset());

                return Results.Ok(new UploadResult(
                    totalBytes,
                    Streamed: true,
                    Limited: true,
                    Sha256: hash));
            });

        return endpoints;
    }

    private sealed record UploadResult(
        long Bytes,
        bool Streamed,
        bool Limited,
        string? Sha256 = null);
}