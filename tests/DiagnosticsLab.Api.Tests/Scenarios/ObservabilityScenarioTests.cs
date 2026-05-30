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
}
