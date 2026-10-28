using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios.RequestExecution;

/// <summary>
/// Contains behavior tests for request execution scenarios (02, 06, 15).
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class RequestExecutionScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
    /// <summary>
    /// Verifies that the problem endpoint reports cancellation-aware behavior as false.
    /// </summary>
    [Fact]
    public async Task MissingCancellationPropagation_Problem_Returns_CancellationAware_False() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/02-missing-cancellation-propagation/problem");

        problem.RootElement.GetProperty("cancellationAware").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint reports cancellation-aware behavior as true.
    /// </summary>
    [Fact]
    public async Task MissingCancellationPropagation_Mitigation_Returns_CancellationAware_True() {
        using var client = factory.CreateClient();

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/02-missing-cancellation-propagation/mitigation");

        mitigation.RootElement.GetProperty("cancellationAware").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problem endpoint reports blocking behavior.
    /// </summary>
    [Fact]
    public async Task BlockingRequestThread_Problem_Returns_Blocking_True() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/06-blocking-request-thread/problem?delayMs=100");

        problem.RootElement.GetProperty("blocking").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint reports blocking behavior as false.
    /// </summary>
    [Fact]
    public async Task BlockingRequestThread_Mitigation_Returns_Blocking_False() {
        using var client = factory.CreateClient();

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/06-blocking-request-thread/mitigation?delayMs=1");

        mitigation.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the problem endpoint reports blocking as true.
    /// </summary>
    [Fact]
    public async Task ThreadPoolStarvation_Problem_Returns_Blocking_True() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/15-threadpool-starvation/problem?delayMs=50");

        problem.RootElement.GetProperty("blocking").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint reports blocking as false.
    /// </summary>
    [Fact]
    public async Task ThreadPoolStarvation_Mitigation_Returns_Blocking_False() {
        using var client = factory.CreateClient();

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/15-threadpool-starvation/mitigation?delayMs=50");

        mitigation.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }
}
