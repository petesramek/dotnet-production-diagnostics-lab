using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Text;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios.Memory;

/// <summary>
/// Contains behavior tests for memory and payload handling scenarios (08, 13, 16).
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class MemoryScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
    /// <summary>
    /// Verifies that the problem endpoint returns row identifiers.
    /// </summary>
    [Fact]
    public async Task LargeResponseBuffering_Problem_Returns_Row_Id() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/14-large-response-buffering/problem?rows=25");

        JsonAssertions.GetIds(problem).Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns the same row identifiers as the problem endpoint.
    /// </summary>
    [Fact]
    public async Task LargeResponseBuffering_Mitigation_Returns_Row_Id() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/14-large-response-buffering/problem?rows=25");

        var mitigation = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/14-large-response-buffering/mitigation?rows=25");

        JsonAssertions.GetIds(mitigation).Should().Equal(JsonAssertions.GetIds(problem));
    }

    /// <summary>
    /// Verifies that the mitigation endpoint streams body and returns a hash.
    /// </summary>
    [Fact]
    public async Task RequestBodyMemoryPressure_Mitigation_Returns_Streamed_True() {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body", Encoding.UTF8, "text/plain");

        using var document = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/15-request-body-memory-pressure/mitigation",
            content);

        document.RootElement.GetProperty("streamed").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("limited").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("sha256").GetString().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that the problem endpoint buffers the request body in memory.
    /// </summary>
    [Fact]
    public async Task RequestBodyMemoryPressure_Problem_Returns_Streamed_False() {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body", Encoding.UTF8, "text/plain");

        using var document = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/15-request-body-memory-pressure/problem",
            content);

        document.RootElement.GetProperty("streamed").GetBoolean().Should().BeFalse();
        document.RootElement.GetProperty("limited").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint rejects bodies larger than the limit.
    /// </summary>
    [Fact]
    public async Task RequestBodyMemoryPressure_Mitigation_Rejects_Body_Larger_Than_Limit() {
        using var client = factory.CreateClient();

        var largeBody = new string('x', (5 * 1024 * 1024) + 1);
        using var content = new StringContent(largeBody, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/15-request-body-memory-pressure/mitigation", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that the problem endpoint processes a generated large payload successfully.
    /// </summary>
    [Fact]
    public async Task LohFragmentation_Problem_Processes_Large_Payload() {
        using var client = factory.CreateClient();

        var payloadResponse = await client.GetAsync("/16-loh-fragmentation/generate");
        payloadResponse.EnsureSuccessStatusCode();

        var json = await payloadResponse.Content.ReadAsStringAsync();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var problem = await client.PostAsync("/16-loh-fragmentation/problem", content);
        problem.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint processes a generated large payload successfully.
    /// </summary>
    [Fact]
    public async Task LohFragmentation_Mitigation_Processes_Large_Payload() {
        using var client = factory.CreateClient();

        var payloadResponse = await client.GetAsync("/16-loh-fragmentation/generate");
        payloadResponse.EnsureSuccessStatusCode();

        var json = await payloadResponse.Content.ReadAsStringAsync();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var mitigation = await client.PostAsync("/16-loh-fragmentation/mitigation", content);
        mitigation.EnsureSuccessStatusCode();
    }
}
