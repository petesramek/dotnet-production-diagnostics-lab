using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios.Startup;

/// <summary>
/// Contains behavior tests for startup and deployment scenarios (11, 18).
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class StartupScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
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
    /// Verifies that the problem endpoint deserializes payload successfully.
    /// </summary>
    [Fact]
    public async Task NativeAotSerializationFailure_Problem_Deserializes_Payload() {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/18-native-aot-serialization-failure/problem",
            new { Name = "Pete", Age = 30 });

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint deserializes payload successfully.
    /// </summary>
    [Fact]
    public async Task NativeAotSerializationFailure_Mitigation_Deserializes_Payload() {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/18-native-aot-serialization-failure/mitigation",
            new { Name = "Pete", Age = 30 });

        response.EnsureSuccessStatusCode();
    }
}
