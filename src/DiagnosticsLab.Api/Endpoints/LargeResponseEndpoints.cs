using System.Runtime.CompilerServices;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for large response allocation and streaming scenarios.
/// </summary>
public static class LargeResponseEndpoints {
    /// <summary>
    /// Adds large response diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapLargeResponseEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/08-large-response");

        group.MapGet("/problem", (int? rows, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("Exports.Problem");

            var normalizedRows = NormalizeRows(rows);

            logger.LogInformation(
                "Building {Rows} export rows in memory before returning response",
                normalizedRows);

            var result = Enumerable.Range(1, normalizedRows)
                .Select(index => new ExportRow(
                    index,
                    $"Customer {index:000}",
                    Math.Round(index * 1.25m, 2)))
                .ToList();

            return Results.Ok(result);
        });

        group.MapGet("/improved", (int? rows, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Exports.Improved");

            var normalizedRows = NormalizeRows(rows);

            logger.LogInformation(
                "Streaming {Rows} export rows without building the full response first",
                normalizedRows);

            return Results.Ok(StreamRowsAsync(normalizedRows, cancellationToken));
        });

        return endpoints;
    }

    private static int NormalizeRows(int? rows) {
        return Math.Clamp(rows ?? 1_000, 1, 50_000);
    }

    private static async IAsyncEnumerable<ExportRow> StreamRowsAsync(
        int rows,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        for (var index = 1; index <= rows; index++) {
            cancellationToken.ThrowIfCancellationRequested();

            if (index % 100 == 0) {
                await Task.Yield();
            }

            yield return new ExportRow(
                index,
                $"Customer {index:000}",
                Math.Round(index * 1.25m, 2));
        }
    }

    private sealed record ExportRow(int Id, string CustomerName, decimal Amount);
}