using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for performance-related diagnostics scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class PerformanceScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the improved endpoint uses non-blocking asynchronous waiting
    /// while the problem endpoint blocks the request thread.
    /// </summary>
    /// <remarks>
    /// The problem endpoint uses Thread.Sleep, which blocks the ThreadPool thread.
    /// The improved endpoint uses Task.Delay, which yields and allows better scalability.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Blocking_problem_and_improved_endpoints_report_correct_behavior() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/06-blocking-request-handling/problem?delayMs=100");

        using var improved = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/06-blocking-request-handling/improved?delayMs=1");

        problem.RootElement
            .GetProperty("blocking")
            .GetBoolean()
            .Should()
            .BeTrue();

        improved.RootElement
            .GetProperty("blocking")
            .GetBoolean()
            .Should()
            .BeFalse();
    }

    /// <summary>
    /// Verifies that both problem and improved endpoints return identical logical
    /// export data for a small dataset.
    /// </summary>
    /// <remarks>
    /// The problem endpoint buffers the entire response in memory,
    /// while the improved endpoint streams results incrementally.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Large_response_problem_and_improved_endpoints_return_same_row_ids() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/08-large-response/problem?rows=25");

        var improved = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/08-large-response/improved?rows=25");

        JsonAssertions.GetIds(problem)
            .Should()
            .Equal(JsonAssertions.GetIds(improved));
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

    /// <summary>
    /// Verifies that the improved upload endpoint streams a small request body and returns a hash.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_improved_endpoint_streams_body_and_returns_hash() {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body", Encoding.UTF8, "text/plain");

        using var document = await JsonTestClient.PostJsonDocumentAsync(client, "/13-request-body-memory-pressure/improved", content);

        document.RootElement.GetProperty("streamed").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("limited").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("sha256").GetString().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that the improved upload endpoint rejects bodies larger than the configured limit.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_improved_endpoint_rejects_body_larger_than_limit() {
        using var client = factory.CreateClient();
        var largeBody = new string('x', (5 * 1024 * 1024) + 1);
        using var content = new StringContent(largeBody, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/13-request-body-memory-pressure/improved", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that the problem endpoint blocks while the improved endpoint uses async.
    /// </summary>
    /// <remarks>
    /// This test validates behavior difference, not full starvation.
    /// Starvation must be observed under load.
    /// </remarks>
    [Fact]
    public async Task Threadpool_starvation_problem_and_improved_report_blocking_state() {
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/15-threadpool-starvation/problem?delayMs=50");

        using var improved = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/15-threadpool-starvation/improved?delayMs=50");

        problem.RootElement.GetProperty("blocking").GetBoolean().Should().BeTrue();
        improved.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that both endpoints process large JSON payloads correctly.
    /// </summary>
    /// <remarks>
    /// This test validates functional correctness.
    /// LOH fragmentation must be observed using runtime diagnostics.
    /// </remarks>
    [Fact]
    public async Task Loh_fragmentation_problem_and_improved_process_large_payload() {
        using var client = factory.CreateClient();

        var payloadResponse = await client.GetAsync("/16-loh-fragmentation/generate");
        payloadResponse.EnsureSuccessStatusCode();

        var json = await payloadResponse.Content.ReadAsStringAsync();

        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var problem = await client.PostAsync("/16-loh-fragmentation/problem", content);
        var improved = await client.PostAsync("/16-loh-fragmentation/improved", content);

        problem.EnsureSuccessStatusCode();
        improved.EnsureSuccessStatusCode();
    }
}
