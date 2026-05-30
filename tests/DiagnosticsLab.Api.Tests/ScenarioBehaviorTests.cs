using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

public sealed class ScenarioBehaviorTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Orders_problem_and_improved_endpoints_return_same_order_ids()
    {
        using var client = factory.CreateClient();

        var slow = await GetJsonArrayAsync(client, "/api/orders/slow?customerId=42");
        var improved = await GetJsonArrayAsync(client, "/api/orders/improved?customerId=42");

        GetIds(slow).Should().Equal(GetIds(improved));
    }

    [Fact]
    public async Task Customers_problem_and_improved_endpoints_return_same_summaries()
    {
        using var client = factory.CreateClient();

        var problem = await GetJsonArrayAsync(client, "/api/customers/problem?take=10");
        var improved = await GetJsonArrayAsync(client, "/api/customers/improved?take=10");

        NormalizeCustomerSummaries(problem).Should().Equal(NormalizeCustomerSummaries(improved));
    }

    [Fact]
    public async Task Shipping_resilient_endpoint_returns_gateway_timeout_for_slow_dependency()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/shipping/resilient?country=SLOW");

        response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
    }

    [Fact]
    public async Task Blocking_improved_endpoint_reports_non_blocking_behavior()
    {
        using var client = factory.CreateClient();

        using var document = await GetJsonDocumentAsync(client, "/api/blocking/improved?delayMs=1");

        document.RootElement.GetProperty("blocking").GetBoolean().Should().BeFalse();
    }

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

    [Fact]
    public async Task Export_problem_and_improved_endpoints_return_same_row_ids_for_small_export()
    {
        using var client = factory.CreateClient();

        var problem = await GetJsonArrayAsync(client, "/api/exports/problem?rows=25");
        var improved = await GetJsonArrayAsync(client, "/api/exports/improved?rows=25");

        GetIds(problem).Should().Equal(GetIds(improved));
    }

    private static async Task<JsonElement> GetJsonArrayAsync(HttpClient client, string requestUri)
    {
        using var document = await GetJsonDocumentAsync(client, requestUri);
        return document.RootElement.Clone();
    }

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

    private static IEnumerable<int> GetIds(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32());
    }

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
