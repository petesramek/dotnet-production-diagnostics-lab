using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for startup exception handling scenarios.
/// </summary>
public static class StartupEndpoints
{
    /// <summary>
    /// Adds startup diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapStartupEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/startup");

        group.MapGet("/problem", async (FakeStartupDependency dependency, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Startup.Problem");

            try
            {
                await dependency.InitializeAsync(shouldFail: true, cancellationToken);
                return Results.Ok(new StartupResult(Initialized: true, FailureVisible: true));
            }
            catch (StartupDependencyException exception)
            {
                logger.LogWarning(exception, "Startup dependency failed, but the failure is being swallowed");
                return Results.Ok(new StartupResult(Initialized: false, FailureVisible: false));
            }
        });

        group.MapGet("/improved", async (FakeStartupDependency dependency, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Startup.Improved");

            try
            {
                await dependency.InitializeAsync(shouldFail: true, cancellationToken);
                return Results.Ok(new StartupResult(Initialized: true, FailureVisible: true));
            }
            catch (StartupDependencyException exception)
            {
                logger.LogError(exception, "Startup dependency failed and the failure is reported clearly");
                return Results.Problem(
                    "Startup dependency initialization failed.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        return endpoints;
    }

    private sealed record StartupResult(bool Initialized, bool FailureVisible);
}
