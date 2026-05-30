using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for performance-related diagnostics scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class PerformanceScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the improved blocking endpoint reports non-blocking behavior.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Blocking_improved_endpoint_reports_non_blocking_behavior()
    {
        using var client = factory.CreateClient();

        using var document = await JsonTestClient.GetJsonDocumentAsync(client, "/api/blocking/improved?delayMs=1");

        document.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the problem and improved export endpoints return the same row identifiers for a small export.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Export_problem_and_improved_endpoints_return_same_row_ids_for_small_export()
    {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(client, "/api/exports/problem?rows=25");
        var improved = await JsonTestClient.GetJsonArrayAsync(client, "/api/exports/improved?rows=25");

        JsonAssertions.GetIds(problem).Should().Equal(JsonAssertions.GetIds(improved));
    }
}
