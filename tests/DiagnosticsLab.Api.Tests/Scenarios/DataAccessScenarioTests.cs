using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for data access diagnostics scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class DataAccessScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the slow and improved orders endpoints return the same logical order identifiers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Orders_problem_and_improved_endpoints_return_same_order_ids() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(client, "/01-slow-data-access/orders/problem?customerId=42");
        var improved = await JsonTestClient.GetJsonArrayAsync(client, "/01-slow-data-access/orders/improved?customerId=42");

        JsonAssertions.GetIds(problem).Should().Equal(JsonAssertions.GetIds(improved));
    }
}
