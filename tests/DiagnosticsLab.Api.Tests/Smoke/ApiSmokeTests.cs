using System.Net;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Smoke;

/// <summary>
/// Contains smoke tests that verify the diagnostics lab starts
/// and exposes every scenario route. Detailed behavior is covered by the scenario tests.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ApiSmokeTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
    /// <summary>
    /// Verifies that the root endpoint returns success.
    /// </summary>
    [Fact]
    public async Task Root_Returns_Success() {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that GET endpoints return the expected status code.
    /// </summary>
    [Theory]
    [InlineData("/01-excessive-data-materialization/problem?customerId=42", HttpStatusCode.OK)]
    [InlineData("/01-excessive-data-materialization/mitigation?customerId=42", HttpStatusCode.OK)]
    [InlineData("/03-missing-cancellation-propagation/problem", HttpStatusCode.OK)]
    [InlineData("/03-missing-cancellation-propagation/mitigation", HttpStatusCode.OK)]
    [InlineData("/09-missing-dependency-timeouts/problem?country=CZ", HttpStatusCode.OK)]
    [InlineData("/09-missing-dependency-timeouts/mitigation?country=CZ", HttpStatusCode.OK)]
    [InlineData("/02-n-plus-one-queries/problem?take=10", HttpStatusCode.OK)]
    [InlineData("/02-n-plus-one-queries/mitigation?take=10", HttpStatusCode.OK)]
    [InlineData("/04-blocking-request-thread/problem?delayMs=100", HttpStatusCode.OK)]
    [InlineData("/04-blocking-request-thread/mitigation?delayMs=1", HttpStatusCode.OK)]
    [InlineData("/10-unbounded-retries/problem?sku=ABC", HttpStatusCode.OK)]
    [InlineData("/10-unbounded-retries/mitigation?sku=ABC", HttpStatusCode.OK)]
    [InlineData("/14-large-response-buffering/problem?rows=10", HttpStatusCode.OK)]
    [InlineData("/14-large-response-buffering/mitigation?rows=10", HttpStatusCode.OK)]
    [InlineData("/17-invalid-configuration/problem", HttpStatusCode.OK)]
    [InlineData("/17-invalid-configuration/mitigation", HttpStatusCode.OK)]
    [InlineData("/18-silent-startup-failure/problem", HttpStatusCode.OK)]
    [InlineData("/18-silent-startup-failure/mitigation", HttpStatusCode.ServiceUnavailable)]
    [InlineData("/11-socket-exhaustion/problem", HttpStatusCode.OK)]
    [InlineData("/11-socket-exhaustion/mitigation", HttpStatusCode.OK)]
    [InlineData("/05-threadpool-starvation/problem?delayMs=10", HttpStatusCode.OK)]
    [InlineData("/05-threadpool-starvation/mitigation?delayMs=10", HttpStatusCode.OK)]
    [InlineData("/16-loh-fragmentation/generate", HttpStatusCode.OK)]
    [InlineData("/12-cache-stampede/problem", HttpStatusCode.OK)]
    [InlineData("/12-cache-stampede/mitigation", HttpStatusCode.OK)]
    [InlineData("/13-repeated-dependency-failures/problem?mode=OK", HttpStatusCode.OK)]
    [InlineData("/13-repeated-dependency-failures/mitigation?mode=OK", HttpStatusCode.OK)]
    public async Task Smoke_Get_Endpoint_Returns_Expected_StatusCode(string requestUri, HttpStatusCode expectedStatusCode) {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    /// <summary>
    /// Verifies that the liveness and readiness health endpoints return success.
    /// </summary>
    [Fact]
    public async Task Health_Endpoints_Return_Success() {
        using var client = factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        live.StatusCode.Should().Be(HttpStatusCode.OK);
        ready.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
