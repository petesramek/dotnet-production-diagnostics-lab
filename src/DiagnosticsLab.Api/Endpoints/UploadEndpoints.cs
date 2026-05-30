using System.Security.Cryptography;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for request body buffering and streaming scenarios.
/// </summary>
public static class UploadEndpoints
{
    private const long MaxUploadBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Adds upload diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/uploads");

        group.MapPost("/problem", async (HttpRequest request, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Uploads.Problem");
            logger.LogInformation("Reading the full request body into memory");

            using var memory = new MemoryStream();
            await request.Body.CopyToAsync(memory, cancellationToken);

            return Results.Ok(new UploadResult(memory.Length, Streamed: false, Limited: false));
        });

        group.MapPost("/improved", async (HttpRequest request, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Uploads.Improved");
            logger.LogInformation("Processing the request body incrementally with a size limit of {MaxUploadBytes} bytes", MaxUploadBytes);

            var buffer = new byte[64 * 1024];
            long totalBytes = 0;
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            while (true)
            {
                var read = await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                {
                    break;
                }

                totalBytes += read;
                if (totalBytes > MaxUploadBytes)
                {
                    logger.LogWarning("Upload exceeded maximum allowed size of {MaxUploadBytes} bytes", MaxUploadBytes);
                    return Results.BadRequest(new { Error = "Upload is too large.", MaxUploadBytes });
                }

                hasher.AppendData(buffer, 0, read);
            }

            var hash = Convert.ToHexString(hasher.GetHashAndReset());
            return Results.Ok(new UploadResult(totalBytes, Streamed: true, Limited: true, Sha256: hash));
        });

        return endpoints;
    }

    private sealed record UploadResult(long Bytes, bool Streamed, bool Limited, string? Sha256 = null);
}
