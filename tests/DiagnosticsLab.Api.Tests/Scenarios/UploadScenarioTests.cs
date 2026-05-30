using System.Net;
using System.Text;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for request body streaming and upload size scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class UploadScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the improved upload endpoint streams a small request body and returns a hash.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_improved_endpoint_streams_body_and_returns_hash()
    {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body", Encoding.UTF8, "text/plain");

        using var document = await JsonTestClient.PostJsonDocumentAsync(client, "/api/uploads/improved", content);

        document.RootElement.GetProperty("streamed").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("limited").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("sha256").GetString().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that the improved upload endpoint rejects bodies larger than the configured limit.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_improved_endpoint_rejects_body_larger_than_limit()
    {
        using var client = factory.CreateClient();
        var largeBody = new string('x', (5 * 1024 * 1024) + 1);
        using var content = new StringContent(largeBody, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/uploads/improved", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
