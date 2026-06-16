using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for reliability and dependency failure scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ReliabilityScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{

    /// <summary>
    /// Verifies that the problem endpoint succeeds for a normal dependency call.
    /// </summary>
    [Fact]
    public async Task MissingDependencyTimeouts_Problem_Returns_Success_For_Normal_Dependency() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/04-missing-dependency-timeouts/problem?country=CZ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns gateway timeout for a slow dependency.
    /// </summary>
    [Fact]
    public async Task MissingDependencyTimeouts_Mitigation_Returns_GatewayTimeout_For_Slow_Dependency() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/04-missing-dependency-timeouts/mitigation?country=SLOW");

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
            "/07-unbounded-retries/problem?sku=FAIL",
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
            "/07-unbounded-retries/problem?sku=FAIL",
            HttpStatusCode.ServiceUnavailable);

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/07-unbounded-retries/mitigation?sku=FAIL",
            HttpStatusCode.ServiceUnavailable);

        var problemAttempts = problem.RootElement.GetProperty("attempts").GetInt32();
        var mitigationAttempts = mitigation.RootElement.GetProperty("attempts").GetInt32();

        mitigationAttempts.Should().BeLessThan(problemAttempts);
        mitigation.RootElement.GetProperty("resilient").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problem endpoint hides startup failure.
    /// </summary>
    [Fact]
    public async Task SilentStartupFailure_Problem_Returns_FailureVisible_False() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/11-silent-startup-failure/problem");

        problem.RootElement.GetProperty("initialized").GetBoolean().Should().BeFalse();
        problem.RootElement.GetProperty("failureVisible").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint surfaces startup failure.
    /// </summary>
    [Fact]
    public async Task SilentStartupFailure_Mitigation_Returns_ServiceUnavailable() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/11-silent-startup-failure/mitigation");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    /// <summary>
    /// Verifies that the problem endpoint returns success.
    /// </summary>
    [Fact]
    public async Task SocketExhaustion_Problem_Returns_Success() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/14-socket-exhaustion/problem");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns success.
    /// </summary>
    [Fact]
    public async Task SocketExhaustion_Mitigation_Returns_Success() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/14-socket-exhaustion/mitigation");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the problem endpoint returns a value.
    /// </summary>
    [Fact]
    public async Task CacheStampede_Problem_Returns_Value() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/17-cache-stampede/problem");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns a value.
    /// </summary>
    [Fact]
    public async Task CacheStampede_Mitigation_Returns_Value() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/17-cache-stampede/mitigation");

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the problem endpoint deserializes payload successfully.
    /// </summary>
    [Fact]
    public async Task NativeAot_Problem_Deserialize_Payload() {
        using var client = factory.CreateClient();

        var payload = new {
            Name = "Pete",
            Age = 30
        };

        var response = await client.PostAsJsonAsync("/18-native-aot/problem", payload);

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint deserializes payload successfully.
    /// </summary>
    [Fact]
    public async Task NativeAot_Mitigation_Deserialize_Payload() {
        using var client = factory.CreateClient();

        var payload = new {
            Name = "Pete",
            Age = 30
        };

        var response = await client.PostAsJsonAsync("/18-native-aot/mitigation", payload);

        response.EnsureSuccessStatusCode();
    }
}
