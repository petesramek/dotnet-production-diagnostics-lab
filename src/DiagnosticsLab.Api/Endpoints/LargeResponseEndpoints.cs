using System.Runtime.CompilerServices;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for large response allocation and streaming scenarios.
/// </summary>
public static class LargeResponseEndpoints {
    private const string Route = "/08-large-response";

    public static IEndpointRouteBuilder MapLargeResponseEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Entire dataset is loaded into memory before returning response.
        group.MapGet("/problem", (int? rows, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("LargeResponse.Problem");

            var normalizedRows = NormalizeRows(rows);

            logger.LogWarning("Building {Rows} rows in memory", normalizedRows);

            // Simulation:
            // Represents loading a large dataset (e.g. from database)
            // fully into memory before returning it.
            var result = Enumerable.Range(1, normalizedRows)
                .Select(i => new ExportRow(
                    i,
                    $"Customer {i:000}",
                    Math.Round(i * 1.25m, 2)))
                .ToList();

            return Results.Ok(result);
        });

        // Mitigation:
        // Stream response incrementally to avoid buffering full dataset.
        group.MapGet("/mitigation", (int? rows, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("LargeResponse.Mitigation");

            var normalizedRows = NormalizeRows(rows);

            logger.LogInformation("Streaming {Rows} rows", normalizedRows);

            // Simulation:
            // Data is produced incrementally instead of allocating full dataset.
            return Results.Ok(StreamRowsAsync(normalizedRows, cancellationToken));
        });

        return endpoints;
    }

    /// <summary>
    /// Normalizes the number of rows to a safe range.
    /// </summary>
    private static int NormalizeRows(int? rows) {
        return Math.Clamp(rows ?? 1000, 1, 50_000);
    }

    /// <summary>
    /// Streams rows incrementally to avoid large allocations.
    /// </summary>
    private static async IAsyncEnumerable<ExportRow> StreamRowsAsync(
        int rows,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        for (var i = 1; i <= rows; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            if (i % 100 == 0) {
                await Task.Yield();
            }

            yield return new ExportRow(
                i,
                $"Customer {i:000}",
                Math.Round(i * 1.25m, 2));
        }
    }

    private sealed record ExportRow(int Id, string CustomerName, decimal Amount);
}

