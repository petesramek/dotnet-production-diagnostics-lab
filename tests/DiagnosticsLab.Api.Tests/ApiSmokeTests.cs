using System.Net;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

public sealed class ApiSmokeTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    [Fact]
    public async Task Root_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Improved_orders_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/improved?customerId=42");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Improved_customers_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customers/improved?take=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Improved_blocking_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/blocking/improved?delayMs=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Improved_inventory_endpoint_returns_success_for_available_sku()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inventory/improved?sku=ABC");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Improved_export_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/exports/improved?rows=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

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
