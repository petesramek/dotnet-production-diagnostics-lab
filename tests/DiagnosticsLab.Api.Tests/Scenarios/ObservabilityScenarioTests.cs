using System.Net;
using System.Net.Http.Json;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for observability and logging failure scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class ObservabilityScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {

    /// <summary>
    /// Verifies that the problem endpoint fails when the audit sink throws.
    /// </summary>
    [Fact]
    public async Task LoggingFailure_Problem_Throws_When_Audit_Fails() {
        using var client = factory.CreateClient();
        var request = new {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        var response = await client.PostAsJsonAsync(
            "/12-logging-failure/problem",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Verifies that the mitigation endpoint continues when the audit sink throws.
    /// </summary>
    [Fact]
    public async Task LoggingFailure_Mitigation_Continue_When_Audit_Fails() {
        using var client = factory.CreateClient();
        var request = new {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        using var response = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/12-logging-failure/mitigation",
            request);

        response.RootElement.GetProperty("businessOperationCompleted").GetBoolean().Should().BeTrue();
        response.RootElement.GetProperty("auditFailureIsolated").GetBoolean().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the problem endpoint returns the basic validation error payload.
    /// </summary>
    [Fact]
    public async Task Observability_Problem_Returns_Error_Payload() {
        using var client = factory.CreateClient();
        var request = new {
            PaymentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CustomerId = 42,
            Amount = -5m
        };

        using var response = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/03-observability-tracing/problem",
            request,
            HttpStatusCode.BadRequest);

        response.RootElement
            .GetProperty("error")
            .GetString()
            .Should()
            .Be("Invalid payment");
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns the contextual validation payload.
    /// </summary>
    [Fact]
    public async Task Observability_Mitigation_Returns_Contextual_Payload() {
        using var client = factory.CreateClient();
        var request = new {
            PaymentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CustomerId = 42,
            Amount = -5m
        };

        using var response = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/03-observability-tracing/mitigation",
            request,
            HttpStatusCode.BadRequest);

        response.RootElement.GetProperty("error").GetString().Should().Be("Invalid payment amount");
        response.RootElement.TryGetProperty("paymentId", out _).Should().BeTrue();
        response.RootElement.TryGetProperty("customerId", out _).Should().BeTrue();
    }
}