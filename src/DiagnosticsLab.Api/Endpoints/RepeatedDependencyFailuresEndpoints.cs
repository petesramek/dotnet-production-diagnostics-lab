using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using System.Net;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for Scenario 19: Repeated Dependency Failures.
/// </summary>
public static class RepeatedDependencyFailuresEndpoints {
    public const string Route = "/19-repeated-dependency-failures";
    public const string ProblemClientName = "RepeatedDependencyFailures.Problem";
    public const string MitigationClientName = "RepeatedDependencyFailures.Mitigation";

    /// <summary>
    /// Maps scenario endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapRepeatedDependencyFailuresEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        group.MapGet("/problem", async (
            string mode,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) => {
                using var client = httpClientFactory.CreateClient(ProblemClientName);
                using var response = await client.GetAsync($"https://dependency.local/repeated-failures?mode={NormalizeMode(mode)}", cancellationToken);

                if (response.IsSuccessStatusCode) {
                    return new ScenarioResult(
                        Mode: NormalizeMode(mode),
                        DependencyCalled: true,
                        CircuitState: "None",
                        Resilient: false,
                        Message: "Dependency succeeded",
                        StatusCode: HttpStatusCode.OK);
                }

                return new ScenarioResult(
                    Mode: NormalizeMode(mode),
                    DependencyCalled: true,
                    CircuitState: "None",
                    Resilient: false,
                    Message: "Dependency failed",
                    StatusCode: HttpStatusCode.ServiceUnavailable);
            });

        group.MapGet("/mitigation", async (
            string mode,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) => {
                try {
                    using var client = httpClientFactory.CreateClient(MitigationClientName);
                    using var response = await client.GetAsync($"https://dependency.local/repeated-failures?mode={NormalizeMode(mode)}", cancellationToken);

                    if (response.IsSuccessStatusCode) {
                        return Results.Json(new ScenarioResult(
                            Mode: NormalizeMode(mode),
                            DependencyCalled: true,
                            CircuitState: "Closed",
                            Resilient: true,
                            Message: "Dependency succeeded",
                            StatusCode: HttpStatusCode.OK));
                    }

                    return Results.Json(new ScenarioResult(
                        Mode: NormalizeMode(mode),
                        DependencyCalled: true,
                        CircuitState: "Closed",
                        Resilient: true,
                        Message: "Dependency failed",
                        StatusCode: HttpStatusCode.ServiceUnavailable));
                } catch (BrokenCircuitException) {
                    return Results.Json(new ScenarioResult(
                        Mode: NormalizeMode(mode),
                        DependencyCalled: false,
                        CircuitState: "Open",
                        Resilient: true,
                        Message: "Call blocked by circuit breaker",
                        StatusCode: HttpStatusCode.ServiceUnavailable));
                }
            });

        return endpoints;
    }

    private static string NormalizeMode(string? mode) =>
        string.IsNullOrWhiteSpace(mode) ? "FAIL" : mode.Trim().ToUpperInvariant();

    /// <summary>
    /// Represents the observable scenario result.
    /// </summary>
    public sealed record ScenarioResult(
        string Mode,
        bool DependencyCalled,
        string CircuitState,
        bool Resilient,
        string Message,
        HttpStatusCode StatusCode);

    public sealed class SimulatedDependencyHandler(ILogger<SimulatedDependencyHandler> logger) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var mode = request.RequestUri is null
                ? "FAIL"
                : QueryHelpers.ParseQuery(request.RequestUri.Query).TryGetValue("mode", out var values)
                    ? values.ToString().Trim().ToUpperInvariant()
                    : "FAIL";

            logger.LogInformation("Simulated dependency invoked for mode {Mode}", mode);

            if (mode == "OK") {
                var success = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = JsonContent.Create(new DependencyPayload(
                        Message: $"Dependency succeeded at {DateTimeOffset.UtcNow:O}",
                        DependencyCalled: true))
                };

                return Task.FromResult(success);
            }

            var failure = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) {
                Content = JsonContent.Create(new DependencyPayload(
                    Message: "Simulated dependency failure",
                    DependencyCalled: true))
            };

            return Task.FromResult(failure);
        }
    }

    public sealed record DependencyPayload(string Message, bool DependencyCalled);
}
