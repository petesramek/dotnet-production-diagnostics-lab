using System.Net;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Smoke;

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
    /// Verifies that representative improved endpoints return successful responses.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData("/01-slow-data-access/orders/improved?customerId=42")]
    [InlineData("/01-slow-data-access/orders/problem?customerId=42")]
    [InlineData("/02-cancellation-timeouts/problem")]
    [InlineData("/02-cancellation-timeouts/improved")]
    [InlineData("/05-n-plus-1-data-access/problem?take=10")]
    [InlineData("/05-n-plus-1-data-access/improved?take=10")]
    [InlineData("/06-blocking-request-handling/problem?delayMs=100")]
    [InlineData("/06-blocking-request-handling/improved?delayMs=1")]
    [InlineData("/07-unbounded-retries/problem?sku=ABC")]
    [InlineData("/07-unbounded-retries/improved?sku=ABC")]
    [InlineData("/api/exports/improved?rows=10")]
    [InlineData("/api/config/improved")]
    public async Task Improved_get_endpoints_return_success(string requestUri)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved upload endpoint returns a successful response for a small upload.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_upload_endpoint_returns_success_for_small_body()
    {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body");

        var response = await client.PostAsync("/api/uploads/improved", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the improved startup endpoint makes initialization failure visible.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_startup_endpoint_returns_service_unavailable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/startup/improved");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
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
