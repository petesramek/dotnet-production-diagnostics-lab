using System.Net;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for reliability and dependency failure scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ReliabilityScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the resilient shipping endpoint returns a gateway timeout for the slow simulated dependency.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Shipping_resilient_endpoint_returns_gateway_timeout_for_slow_dependency()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/04-external-dependency-reliability/improved?country=SLOW");

        response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
    }

    /// <summary>
    /// Verifies that the improved inventory endpoint uses fewer attempts than the problematic endpoint for a failed SKU.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Inventory_improved_endpoint_uses_fewer_attempts_than_problem_endpoint_for_failed_sku()
    {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(client, "/api/inventory/problem?sku=FAIL", HttpStatusCode.ServiceUnavailable);
        using var improved = await JsonTestClient.GetJsonDocumentAsync(client, "/api/inventory/improved?sku=FAIL", HttpStatusCode.ServiceUnavailable);

        var problemAttempts = problem.RootElement.GetProperty("attempts").GetInt32();
        var improvedAttempts = improved.RootElement.GetProperty("attempts").GetInt32();

        improvedAttempts.Should().BeLessThan(problemAttempts);
        improved.RootElement.GetProperty("resilient").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problematic startup endpoint hides initialization failure while the improved endpoint exposes it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Startup_problem_hides_failure_while_improved_endpoint_exposes_failure()
    {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(client, "/api/startup/problem");
        var improved = await client.GetAsync("/api/startup/improved");

        problem.RootElement.GetProperty("initialized").GetBoolean().Should().BeFalse();
        problem.RootElement.GetProperty("failureVisible").GetBoolean().Should().BeFalse();
        improved.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
