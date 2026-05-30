using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

/// <summary>
/// Contains behavior tests for milestone 5 scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class Milestone5ScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the problematic startup endpoint hides initialization failure while the improved endpoint exposes it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Startup_problem_hides_failure_while_improved_endpoint_exposes_failure()
    {
        using var client = factory.CreateClient();

        using var problem = await GetJsonDocumentAsync(client, "/api/startup/problem");
        var improved = await client.GetAsync("/api/startup/improved");

        problem.RootElement.GetProperty("initialized").GetBoolean().Should().BeFalse();
        problem.RootElement.GetProperty("failureVisible").GetBoolean().Should().BeFalse();
        improved.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    /// <summary>
    /// Verifies that non-critical audit sink failures are isolated by the improved endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Audit_improved_endpoint_isolates_non_critical_audit_failure()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        var problem = await client.PostAsJsonAsync("/api/audit/problem", request);
        using var improved = await PostJsonDocumentAsync(client, "/api/audit/improved", request);

        problem.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        improved.RootElement.GetProperty("businessOperationCompleted").GetBoolean().Should().BeTrue();
        improved.RootElement.GetProperty("auditFailureIsolated").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the improved upload endpoint streams a small request body and returns a hash.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_improved_endpoint_streams_body_and_returns_hash()
    {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body", Encoding.UTF8, "text/plain");

        using var document = await PostJsonDocumentAsync(client, "/api/uploads/improved", content);

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

    /// <summary>
    /// Sends a GET request and parses the successful JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <returns>The parsed JSON document.</returns>
    private static async Task<JsonDocument> GetJsonDocumentAsync(HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// Sends a JSON POST request and parses the successful JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="value">The value to send as JSON.</param>
    /// <returns>The parsed JSON document.</returns>
    private static async Task<JsonDocument> PostJsonDocumentAsync(HttpClient client, string requestUri, object value)
    {
        var response = await client.PostAsJsonAsync(requestUri, value);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// Sends a POST request and parses the successful JSON response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content.</param>
    /// <returns>The parsed JSON document.</returns>
    private static async Task<JsonDocument> PostJsonDocumentAsync(HttpClient client, string requestUri, HttpContent content)
    {
        var response = await client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
