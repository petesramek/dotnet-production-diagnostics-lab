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
public sealed class ObservabilityScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory>
{
    /// <summary>
    /// Verifies that non-critical audit sink failures are isolated by the improved endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Audit_improved_endpoint_isolates_non_critical_audit_failure()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            OperationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            AuditShouldFail = true
        };

        var problem = await client.PostAsJsonAsync("/api/audit/problem", request);
        using var improved = await JsonTestClient.PostJsonDocumentAsync(client, "/api/audit/improved", request);

        problem.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        improved.RootElement.GetProperty("businessOperationCompleted").GetBoolean().Should().BeTrue();
        improved.RootElement.GetProperty("auditFailureIsolated").GetBoolean().Should().BeTrue();
    }

    /// <summary>/// consistent business results
    /// while providing different levels of observability context.
    /// </summary>
    /// <remarks>
    /// The improved endpoint includes additional identifiers in the response when validation fails,
    /// which allows easier correlation with logs.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Observability_problem_and_improved_endpoints_return_expected_payloads() {
        using var client = factory.CreateClient();

        var request = new {
            PaymentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CustomerId = 42,
            Amount = -5m
        };

        using var problem = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/03-observability-tracing/problem",
            request,
            HttpStatusCode.BadRequest);

        using var improved = await JsonTestClient.PostJsonDocumentAsync(
            client,
            "/03-observability-tracing/improved",
            request,
            HttpStatusCode.BadRequest);

        problem.RootElement.GetProperty("error").GetString()
            .Should()
            .Be("Invalid payment");

        improved.RootElement.GetProperty("error").GetString()
            .Should()
            .Be("Invalid payment amount");

        improved.RootElement.TryGetProperty("paymentId", out _).Should().BeTrue();
        improved.RootElement.TryGetProperty("customerId", out _).Should().BeTrue();
    }
}
