using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DiagnosticsLab.Api.Scenarios.Observability;

/// <summary>
/// Registers and maps the health probes for Scenario 07: Missing Health Probes.
/// </summary>
public static class MissingHealthProbesEndpoints {
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";

    public static IServiceCollection AddMissingHealthProbesServices(this IServiceCollection services) {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."), tags: ["live"])
            .AddCheck("dependencies", () => HealthCheckResult.Healthy("Dependencies are reachable."), tags: ["ready"]);

        return services;
    }

    public static IEndpointRouteBuilder MapMissingHealthProbesEndpoints(this IEndpointRouteBuilder endpoints) {
        // Framework solution: tag checks and filter per probe so liveness never depends on dependencies.
        endpoints.MapHealthChecks(LivePath, new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
        endpoints.MapHealthChecks(ReadyPath, new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

        return endpoints;
    }
}
