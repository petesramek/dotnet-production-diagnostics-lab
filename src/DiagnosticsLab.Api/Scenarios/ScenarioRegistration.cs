using DiagnosticsLab.Api.Configuration;
using DiagnosticsLab.Api.Data;
using DiagnosticsLab.Api.Scenarios.DataAccess;
using DiagnosticsLab.Api.Scenarios.DependencyReliability;
using DiagnosticsLab.Api.Scenarios.Memory;
using DiagnosticsLab.Api.Scenarios.Observability;
using DiagnosticsLab.Api.Scenarios.RequestExecution;
using DiagnosticsLab.Api.Scenarios.Startup;
using DiagnosticsLab.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsLab.Api.Scenarios;

/// <summary>
/// Single entry point that wires every scenario into the host.
/// </summary>
public static class ScenarioRegistration {
    public static IServiceCollection AddScenarios(this IServiceCollection services, IConfiguration configuration) {
        // Scenario 09: Invalid Configuration
        services
            .AddOptions<ExternalServicesOptions>()
            .Bind(configuration.GetSection(ExternalServicesOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => Uri.TryCreate(options.BillingApiBaseUrl, UriKind.Absolute, out _), "Billing API base URL must be an absolute URI.")
            .ValidateOnStart();

        // Scenarios 01 and 05: data access
        services.AddDbContext<AppDbContext>(options => {
            var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=diagnostics-lab.db";
            options.UseSqlite(connectionString);
        });

        // Scenarios 04 and 07: simulated external dependencies
        services.AddScoped<FakeShippingClient>();
        services.AddScoped<FakeInventoryClient>();

        // Scenario 11: startup dependency
        services.AddSingleton<FakeStartupDependency>();

        // Scenario 12: audit sink
        services.AddSingleton<FakeAuditSink>();

        // Scenarios 03 and 13: request context access
        services.AddHttpContextAccessor();

        // Scenarios 14 and 17: HttpClient factory and in-memory cache
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddHybridCache();

        services.AddMissingHealthProbesServices();
        services.AddRepeatedDependencyFailuresServices();
        services.AddSocketExhaustionServices();

        return services;
    }

    public static IEndpointRouteBuilder MapScenarios(this IEndpointRouteBuilder endpoints) {
        // Data access
        endpoints.MapExcessiveDataMaterializationEndpoints();
        endpoints.MapNPlusOneQueryEndpoints();

        // Request execution
        endpoints.MapMissingCancellationPropagationEndpoints();
        endpoints.MapBlockingRequestThreadEndpoints();
        endpoints.MapThreadPoolStarvationEndpoints();

        // Observability
        endpoints.MapMissingLogContextEndpoints();
        endpoints.MapMissingHealthProbesEndpoints();
        endpoints.MapLoggingAndAuditFailurePropagationEndpoints();

        // Dependency reliability
        endpoints.MapMissingDependencyTimeoutsEndpoints();
        endpoints.MapUnboundedRetriesEndpoints();
        endpoints.MapSocketExhaustionEndpoints();
        endpoints.MapCacheStampedeEndpoints();
        endpoints.MapRepeatedDependencyFailuresEndpoints();

        // Memory and payload handling
        endpoints.MapLargeResponseBufferingEndpoints();
        endpoints.MapRequestBodyMemoryPressureEndpoints();
        endpoints.MapLohFragmentationEndpoints();

        // Startup, configuration, deployment
        endpoints.MapInvalidConfigurationEndpoints();
        endpoints.MapSilentStartupFailureEndpoints();
        endpoints.MapNativeAotSerializationFailureEndpoints();

        return endpoints;
    }
}
