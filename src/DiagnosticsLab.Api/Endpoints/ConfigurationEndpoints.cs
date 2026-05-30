using DiagnosticsLab.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DiagnosticsLab.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/config");

        group.MapGet("/problem", (IConfiguration configuration, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Configuration.Problem");
            var billingApiBaseUrl = configuration["ExternalServices:BillingApiBaseUrl"];

            if (string.IsNullOrWhiteSpace(billingApiBaseUrl))
            {
                logger.LogError("Billing API base URL is missing at request time");
                return Results.Problem("Billing API base URL is missing.", statusCode: StatusCodes.Status500InternalServerError);
            }

            return Results.Ok(new { BillingApiBaseUrl = billingApiBaseUrl, ValidatedAtStartup = false });
        });

        group.MapGet("/improved", (IOptions<ExternalServicesOptions> options) =>
        {
            return Results.Ok(new
            {
                options.Value.BillingApiBaseUrl,
                ValidatedAtStartup = true
            });
        });

        return endpoints;
    }
}
