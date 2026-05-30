using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

public sealed class ApiSmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
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
}
