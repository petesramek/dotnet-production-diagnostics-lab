using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

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
}
