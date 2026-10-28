using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios.DependencyReliability;

/// <summary>
/// Contains behavior tests for dependency reliability scenarios (04, 07, 14, 17, 19).
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class DependencyReliabilityScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
    /// <summary>
    /// Verifies that the mitigation coordinates concurrent requests into a single computation.
    /// </summary>
    [Fact]
    public async Task CacheStampede_Mitigation_Returns_Same_Value_For_Concurrent_Requests() {
        using var client = factory.CreateClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => client.GetStringAsync("/12-cache-stampede/mitigation")));

        var values = responses
            .Select(body => System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("value").GetString())
            .Distinct();

        values.Should().ContainSingle();
    }

    /// <summary>
    /// Verifies that the problem endpoint succeeds for a normal dependency call.
    /// </summary>
    [Fact]
    public async Task MissingDependencyTimeouts_Problem_Returns_Success_For_Normal_Dependency() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/09-missing-dependency-timeouts/problem?country=CZ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns gateway timeout for a slow dependency.
    /// </summary>
    [Fact]
    public async Task MissingDependencyTimeouts_Mitigation_Returns_GatewayTimeout_For_Slow_Dependency() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/09-missing-dependency-timeouts/mitigation?country=SLOW");

        response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
    }

    /// <summary>
    /// Verifies that the problem endpoint returns a failed result for a failing SKU.
    /// </summary>
    [Fact]
    public async Task UnboundedRetries_Problem_Returns_ServiceUnavailable_For_Failed_Sku() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/10-unbounded-retries/problem?sku=FAIL",
            HttpStatusCode.ServiceUnavailable);

        problem.RootElement.GetProperty("attempts").GetInt32().Should().BeGreaterThan(0);
        problem.RootElement.GetProperty("resilient").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint uses fewer attempts for a failing SKU.
    /// </summary>
    [Fact]
    public async Task UnboundedRetries_Mitigation_Returns_Fewer_Attempts_For_Failed_Sku() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/10-unbounded-retries/problem?sku=FAIL",
            HttpStatusCode.ServiceUnavailable);

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/10-unbounded-retries/mitigation?sku=FAIL",
            HttpStatusCode.ServiceUnavailable);

        var problemAttempts = problem.RootElement.GetProperty("attempts").GetInt32();
        var mitigationAttempts = mitigation.RootElement.GetProperty("attempts").GetInt32();

        mitigationAttempts.Should().BeLessThan(problemAttempts);
        mitigation.RootElement.GetProperty("resilient").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problem endpoint returns success.
    /// </summary>
    [Fact]
    public async Task SocketExhaustion_Problem_Returns_Success() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/11-socket-exhaustion/problem");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns success.
    /// </summary>
    [Fact]
    public async Task SocketExhaustion_Mitigation_Returns_Success() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/11-socket-exhaustion/mitigation");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the problem endpoint returns a value.
    /// </summary>
    [Fact]
    public async Task CacheStampede_Problem_Returns_Value() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/12-cache-stampede/problem");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns a value.
    /// </summary>
    [Fact]
    public async Task CacheStampede_Mitigation_Returns_Value() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/12-cache-stampede/mitigation");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the problem endpoint keeps calling a failing dependency.
    /// </summary>
    [Fact]
    public async Task RepeatedDependencyFailures_Problem_Always_Calls_Dependency() {
        using var client = factory.CreateClient();

        for (var i = 0; i < 5; i++) {
            using var problem = await JsonTestClient.GetJsonDocumentAsync(
                client,
                "/13-repeated-dependency-failures/problem?mode=FAIL",
                HttpStatusCode.ServiceUnavailable);

            problem.RootElement.GetProperty("dependencyCalled").GetBoolean().Should().BeTrue();
            problem.RootElement.GetProperty("circuitState").GetString().Should().Be("None");
        }
    }

    /// <summary>
    /// Verifies that the mitigation endpoint fails fast once the circuit opens.
    /// </summary>
    [Fact]
    public async Task RepeatedDependencyFailures_Mitigation_Fails_Fast_After_Threshold() {
        using var isolatedFactory = new DiagnosticsLabWebApplicationFactory();
        using var client = isolatedFactory.CreateClient();

        for (var i = 0; i < 3; i++) {
            using var failing = await JsonTestClient.GetJsonDocumentAsync(
                client,
                "/13-repeated-dependency-failures/mitigation?mode=FAIL",
                HttpStatusCode.ServiceUnavailable);

            failing.RootElement.GetProperty("dependencyCalled").GetBoolean().Should().BeTrue();
        }

        using var blocked = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/13-repeated-dependency-failures/mitigation?mode=FAIL",
            HttpStatusCode.ServiceUnavailable);

        blocked.RootElement.GetProperty("dependencyCalled").GetBoolean().Should().BeFalse();
        blocked.RootElement.GetProperty("circuitState").GetString().Should().Be("Open");
    }
}
