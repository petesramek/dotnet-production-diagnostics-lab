using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for silent startup failure scenarios.
/// </summary>
public static class SilentStartupFailureEndpoints {
    /// <summary>
    /// Adds silent startup failure diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapSilentStartupFailureEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/11-silent-startup-failure");

        group.MapGet("/problem", async (FakeStartupDependency dependency, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Startup.Problem");

            try {
                await dependency.InitializeAsync(shouldFail: true, cancellationToken);

                return Results.Ok(new StartupResult(Initialized: true, FailureVisible: true));
            } catch (StartupDependencyException exception) {
                logger.LogWarning(
                    exception,
                    "Startup dependency failed, but the failure is being swallowed");

                return Results.Ok(new StartupResult(Initialized: false, FailureVisible: false));
            }
        });

        group.MapGet("/improved", async (FakeStartupDependency dependency, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Startup.Improved");

            try {
                await dependency.InitializeAsync(shouldFail: true, cancellationToken);

                return Results.Ok(new StartupResult(Initialized: true, FailureVisible: true));
            } catch (StartupDependencyException exception) {
                logger.LogError(
                    exception,
                    "Startup dependency failed and the failure is reported clearly");

                return Results.Problem(
                    "Startup dependency initialization failed.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        return endpoints;
    }

    private sealed record StartupResult(bool Initialized, bool FailureVisible);
}