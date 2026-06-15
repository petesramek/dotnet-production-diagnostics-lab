using DiagnosticsLab.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for missing configuration validation scenarios.
/// </summary>
public static class InvalidConfigurationEndpoints {
    private const string Route = "/09-invalid-configuration";

    public static IEndpointRouteBuilder MapInvalidConfigurationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Configuration is accessed at runtime without validation.
        group.MapGet("/problem", (IConfiguration configuration, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("InvalidConfiguration.Problem");

            var billingApiBaseUrl = configuration["ExternalServices:BillingApiBaseUrl"];

            // Accessing raw configuration without validation:
            // - errors are detected only during request execution
            if (string.IsNullOrWhiteSpace(billingApiBaseUrl)) {
                logger.LogError("Billing API base URL is missing at request time");

                return Results.Problem(
                    "Billing API base URL is missing.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            return Results.Ok(new {
                BillingApiBaseUrl = billingApiBaseUrl,
                Validated = false
            });
        });

        // Mitigation:
        // Configuration is validated at startup and safe to use at runtime.
        group.MapGet("/mitigation", (IOptions<ExternalServicesOptions> options, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("InvalidConfiguration.Mitigation");

            logger.LogInformation("Using configuration validated at startup");

            // Configuration was validated during startup:
            // - invalid config prevents application start
            var billingApiBaseUrl = options.Value.BillingApiBaseUrl;

            return Results.Ok(new {
                BillingApiBaseUrl = billingApiBaseUrl,
                Validated = true
            });
        });

        return endpoints;
    }
}