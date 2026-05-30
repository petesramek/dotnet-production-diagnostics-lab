using System.Text.Json;

namespace DiagnosticsLab.Api.Tests.Infrastructure;

/// <summary>
/// Provides JSON projection helpers for scenario assertions.
/// </summary>
internal static class JsonAssertions
{
    /// <summary>
    /// Extracts integer identifiers from an array of JSON objects.
    /// </summary>
    /// <param name="array">The JSON array.</param>
    /// <returns>The identifiers from the JSON objects.</returns>
    public static IEnumerable<int> GetIds(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32());
    }

    /// <summary>
    /// Converts customer summary JSON objects to comparable string representations.
    /// </summary>
    /// <param name="array">The JSON array.</param>
    /// <returns>The normalized customer summaries.</returns>
    public static IEnumerable<string> NormalizeCustomerSummaries(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(item => string.Join('|',
                item.GetProperty("id").GetInt32(),
                item.GetProperty("name").GetString(),
                item.GetProperty("segment").GetString(),
                item.GetProperty("orderCount").GetInt32(),
                item.GetProperty("totalSpent").GetDecimal()));
    }
}
