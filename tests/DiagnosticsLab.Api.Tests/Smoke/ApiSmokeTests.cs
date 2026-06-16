using System.Net;
using System.Net.Http.Json;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Smoke;

/// <summary>
/// Contains smoke tests that verify the diagnostics lab starts
/// and exposes representative endpoints.
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
    /// Verifies that representative GET endpoints return the expected status code.
    /// </summary>
    [Theory]
    [InlineData("/01-excessive-data-materialization/problem?customerId=42", HttpStatusCode.OK)]
    [InlineData("/01-excessive-data-materialization/mitigation?customerId=42", HttpStatusCode.OK)]
    [InlineData("/02-missing-cancellation-propagation/problem", HttpStatusCode.OK)]
    [InlineData("/02-missing-cancellation-propagation/mitigation", HttpStatusCode.OK)]
    [InlineData("/04-external-dependency-reliability/problem?country=CZ", HttpStatusCode.OK)]
    [InlineData("/04-external-dependency-reliability/mitigation?country=CZ", HttpStatusCode.OK)]
    [InlineData("/05-n-plus-1-data-access/problem?take=10", HttpStatusCode.OK)]
    [InlineData("/05-n-plus-1-data-access/mitigation?take=10", HttpStatusCode.OK)]
    [InlineData("/06-blocking-request-handling/problem?delayMs=100", HttpStatusCode.OK)]
    [InlineData("/06-blocking-request-handling/mitigation?delayMs=1", HttpStatusCode.OK)]
    [InlineData("/07-unbounded-retries/problem?sku=ABC", HttpStatusCode.OK)]
    [InlineData("/07-unbounded-retries/mitigation?sku=ABC", HttpStatusCode.OK)]
    [InlineData("/08-large-response/problem?rows=10", HttpStatusCode.OK)]
    [InlineData("/08-large-response/mitigation?rows=10", HttpStatusCode.OK)]
    [InlineData("/09-invalid-configuration/problem", HttpStatusCode.OK)]
    [InlineData("/09-invalid-configuration/mitigation", HttpStatusCode.OK)]
    [InlineData("/11-silent-startup-failure/problem", HttpStatusCode.OK)]
    [InlineData("/11-silent-startup-failure/mitigation", HttpStatusCode.ServiceUnavailable)]
    [InlineData("/14-socket-exhaustion/problem", HttpStatusCode.OK)]
    [InlineData("/14-socket-exhaustion/mitigation", HttpStatusCode.OK)]
    [InlineData("/15-threadpool-starvation/problem?delayMs=10", HttpStatusCode.OK)]
    [InlineData("/15-threadpool-starvation/mitigation?delayMs=10", HttpStatusCode.OK)]
    [InlineData("/16-loh-fragmentation/generate", HttpStatusCode.OK)]
    [InlineData("/17-cache-stampede/problem", HttpStatusCode.OK)]
    [InlineData("/17-cache-stampede/mitigation", HttpStatusCode.OK)]
    public async Task Smoke_Get_Endpoint_Returns_Expected_StatusCode(string requestUri, HttpStatusCode expectedStatusCode) {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    /// <summary>
    /// Verifies that the problem observability endpoint returns bad request for invalid payment data.
    /// </summary>
    [Fact]
    public async Task MissingLogContext_Problem_Returns_BadRequest() {
        using var client = factory.CreateClient();

        var request = new {
            PaymentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CustomerId = 42,
            Amount = -5m
        };

        var response = await client.PostAsJsonAsync("/03-missing-log-context-tracing/problem", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that the mitigation observability endpoint returns bad request for invalid payment data.
    /// </summary>
    [Fact]
    public async Task MissingLogContext_Mitigation_Returns_BadRequest() {
        using var client = factory.CreateClient();

        var request = new {
            PaymentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CustomerId = 42,
            Amount = -5m
        };

        var response = await client.PostAsJsonAsync("/03-missing-log-context-tracing/mitigation", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that the problem logging endpoint fails when audit sink throws.
    /// </summary>
    [Fact]
    public async Task LoggingFailure_Problem_Returns_InternalServerError() {
        using var client = factory.CreateClient();

        var request = new {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        var response = await client.PostAsJsonAsync("/12-logging-failure/problem", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Verifies that the mitigation logging endpoint succeeds when audit sink throws.
    /// </summary>
    [Fact]
    public async Task LoggingFailure_Mitigation_Returns_Success() {
        using var client = factory.CreateClient();

        var request = new {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        var response = await client.PostAsJsonAsync("/12-logging-failure/mitigation", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the problem upload endpoint returns success for a small request body.
    /// </summary>
    [Fact]
    public async Task RequestBodyMemoryPressure_Problem_Returns_Success_For_Small_Body() {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body");

        var response = await client.PostAsync("/13-request-body-memory-pressure/problem", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the mitigation upload endpoint returns success for a small request body.
    /// </summary>
    [Fact]
    public async Task RequestBodyMemoryPressure_Mitigation_Returns_Success_For_Small_Body() {
        using var client = factory.CreateClient();
        using var content = new StringContent("small upload body");

        var response = await client.PostAsync("/13-request-body-memory-pressure/mitigation", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the problem native AOT endpoint accepts a valid payload.
    /// </summary>
    [Fact]
    public async Task NativeAot_Problem_Returns_Success() {
        using var client = factory.CreateClient();

        var payload = new {
            Name = "Pete",
            Age = 30
        };

        var response = await client.PostAsJsonAsync("/18-native-aot/problem", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the mitigation native AOT endpoint accepts a valid payload.
    /// </summary>
    [Fact]
    public async Task NativeAot_Mitigation_Returns_Success() {
        using var client = factory.CreateClient();

        var payload = new {
            Name = "Pete",
            Age = 30
        };

        var response = await client.PostAsJsonAsync("/18-native-aot/mitigation", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the liveness and readiness health endpoints return success.
    /// </summary>
    [Fact]
    public async Task Health_Endpoints_Returns_Success() {
        using var client = factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        live.StatusCode.Should().Be(HttpStatusCode.OK);
        ready.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}