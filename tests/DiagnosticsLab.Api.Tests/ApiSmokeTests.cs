using System.Net;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

/// <summary>
/// Contains smoke tests that verify the diagnostics lab starts and exposes its main improved endpoints.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ApiSmokeTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that the root endpoint returns a successful response.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Root_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved orders endpoint returns a successful response.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_orders_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/improved?customerId=42");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved customers endpoint returns a successful response.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_customers_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customers/improved?take=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved blocking endpoint returns a successful response.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_blocking_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blocking/improved?delayMs=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved inventory endpoint returns a successful response for an available SKU.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_inventory_endpoint_returns_success_for_available_sku()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inventory/improved?sku=ABC");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved export endpoint returns a successful response.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_export_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/exports/improved?rows=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the liveness and readiness health endpoints return successful responses.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Health_endpoints_return_success()
    {
        using var client = factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        live.StatusCode.Should().Be(HttpStatusCode.OK);
        ready.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
