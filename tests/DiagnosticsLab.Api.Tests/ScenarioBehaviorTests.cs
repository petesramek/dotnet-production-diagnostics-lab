using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

/// <summary>
/// Contains behavior tests that compare problem and improved implementations for selected diagnostics scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ScenarioBehaviorTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the slow and improved orders endpoints return the same logical order identifiers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Orders_problem_and_improved_endpoints_return_same_order_ids()
    {
        using var client = factory.CreateClient();

        var slow = await GetJsonArrayAsync(client, "/api/orders/slow?customerId=42");
        var improved = await GetJsonArrayAsync(client, "/api/orders/improved?customerId=42");

        GetIds(slow).Should().Equal(GetIds(improved));
    }

    /// <summary>
    /// Verifies that the chatty and improved customers endpoints return the same logical customer summaries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Customers_problem_and_improved_endpoints_return_same_summaries()
    {
        using var client = factory.CreateClient();

        var problem = await GetJsonArrayAsync(client, "/api/customers/problem?take=10");
        var improved = await GetJsonArrayAsync(client, "/api/customers/improved?take=10");

        NormalizeCustomerSummaries(problem).Should().Equal(NormalizeCustomerSummaries(improved));
    }

    /// <summary>
    /// Verifies that the resilient shipping endpoint returns a gateway timeout for the slow simulated dependency.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Shipping_resilient_endpoint_returns_gateway_timeout_for_slow_dependency()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/shipping/resilient?country=SLOW");

        response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
    }

    /// <summary>
    /// Verifies that the improved blocking endpoint reports non-blocking behavior.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Blocking_improved_endpoint_reports_non_blocking_behavior()
    {
        using var client = factory.CreateClient();

        using var document = await GetJsonDocumentAsync(client, "/api/blocking/improved?delayMs=1");

        document.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the improved inventory endpoint uses fewer attempts than the problematic endpoint for a failed SKU.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Inventory_improved_endpoint_uses_fewer_attempts_than_problem_endpoint_for_failed_sku()
    {
        using var client = factory.CreateClient();

        using var problem = await GetJsonDocumentAsync(client, "/api/inventory/problem?sku=FAIL", HttpStatusCode.ServiceUnavailable);
        using var improved = await GetJsonDocumentAsync(client, "/api/inventory/improved?sku=FAIL", HttpStatusCode.ServiceUnavailable);

        var problemAttempts = problem.RootElement.GetProperty("attempts").GetInt32();
        var improvedAttempts = improved.RootElement.GetProperty("attempts").GetInt32();

        improvedAttempts.Should().BeLessThan(problemAttempts);
        improved.RootElement.GetProperty("resilient").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problem and improved export endpoints return the same row identifiers for a small export.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Export_problem_and_improved_endpoints_return_same_row_ids_for_small_export()
    {
        using var client = factory.CreateClient();

        var problem = await GetJsonArrayAsync(client, "/api/exports/problem?rows=25");
        var improved = await GetJsonArrayAsync(client, "/api/exports/improved?rows=25");

        GetIds(problem).Should().Equal(GetIds(improved));
    }

    /// <summary>
    /// Sends a request and parses the successful JSON array response.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <returns>The cloned JSON array root element.</returns>
    private static async Task<JsonElement> GetJsonArrayAsync(HttpClient client, string requestUri)
    {
        using var document = await GetJsonDocumentAsync(client, requestUri);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Sends a request and parses the JSON response, asserting the expected status code.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>The parsed JSON document.</returns>
    private static async Task<JsonDocument> GetJsonDocumentAsync(
        HttpClient client,
        string requestUri,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        var response = await client.GetAsync(requestUri);
        response.StatusCode.Should().Be(expectedStatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// Extracts integer identifiers from an array of JSON objects.
    /// </summary>
    /// <param name="array">The JSON array.</param>
    /// <returns>The identifiers from the JSON objects.</returns>
    private static IEnumerable<int> GetIds(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32());
    }

    /// <summary>
    /// Converts customer summary JSON objects to comparable string representations.
    /// </summary>
    /// <param name="array">The JSON array.</param>
    /// <returns>The normalized customer summaries.</returns>
    private static IEnumerable<string> NormalizeCustomerSummaries(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(item => string.Join('|',
                item.GetProperty("id").GetInt32(),
                item.GetProperty("name").GetString(),
                item.GetProperty("segment").GetString(),
                item.GetProperty("orderCount").GetInt32(),
                item.GetProperty("totalSpent").GetDecimal()));
    }
}
