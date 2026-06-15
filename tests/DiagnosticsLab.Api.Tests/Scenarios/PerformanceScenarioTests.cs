using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Reflection.Metadata;
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

        using var problem = await JsonTestClient.GetJsonDocumentAsync(client, "/api/blocking/problem?delayMs=100");
        using var improved = await JsonTestClient.GetJsonDocumentAsync(client, "/api/blocking/improved?delayMs=1");

        problem.RootElement.GetProperty("blocking").GetBoolean().Should().BeTrue();
        improved.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
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


    /// <summary>
    /// Verifies that the problem endpoint ignores cancellation while the improved endpoint reports cancellation-aware behavior.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Cancellation_problem_and_improved_return_expected_flags() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(client, "/02-cancellation-timeouts/problem");
        using var improved = await JsonTestClient.GetJsonDocumentAsync(client, "/02-cancellation-timeouts/improved");

        problem.RootElement.GetProperty("cancellationAware").GetBoolean().Should().BeFalse();
        improved.RootElement.GetProperty("cancellationAware").GetBoolean().Should().BeTrue();
    }
}
