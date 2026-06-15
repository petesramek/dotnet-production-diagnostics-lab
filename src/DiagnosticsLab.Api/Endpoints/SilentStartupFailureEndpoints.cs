using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for silent startup failure scenarios.
/// </summary>
public static class SilentStartupFailureEndpoints {
    private const string Route = "/11-silent-startup-failure";

    public static IEndpointRouteBuilder MapSilentStartupFailureEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Startup failure is swallowed and the application continues running
        // in a partially initialized (broken) state.
        group.MapGet("/problem", async (
            FakeStartupDependency dependency,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SilentStartupFailure.Problem");

                try {
                    // Simulation:
                    // This represents application startup initialization logic, such as:
                    // - establishing connections to external systems
                    // - initializing background services
                    // - preparing critical dependencies
                    //
                    // In a real application, this code would run during startup,
                    // not inside an HTTP request. It is placed here to make the
                    // scenario observable and testable.
                    await dependency.InitializeAsync(shouldFail: true, cancellationToken);

                    return Results.Ok(new StartupResult(
                        Initialized: true,
                        FailureVisible: true));
                } catch (StartupDependencyException exception) {
                    logger.LogWarning(exception,
                        "Startup dependency failed but the error is swallowed");

                    // Simulation:
                    // Swallowing the exception represents incorrect startup handling where:
                    // - initialization failure is ignored
                    // - application continues running in an invalid state
                    //
                    // In real systems, this leads to:
                    // - hidden failures
                    // - delayed runtime errors
                    // - hard-to-diagnose issues
                    return Results.Ok(new StartupResult(
                        Initialized: false,
                        FailureVisible: false));
                }
            });

        // Mitigation:
        // Fail fast when critical startup dependencies fail instead of continuing.
        group.MapGet("/mitigation", async (
            FakeStartupDependency dependency,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SilentStartupFailure.Mitigation");

                try {
                    // Simulation:
                    // Same startup initialization logic as in the problem case.
                    // In real applications, this would be executed during host startup.
                    await dependency.InitializeAsync(shouldFail: true, cancellationToken);

                    return Results.Ok(new StartupResult(
                        Initialized: true,
                        FailureVisible: true));
                } catch (StartupDependencyException exception) {
                    logger.LogError(exception,
                        "Startup dependency failed — preventing application from running");

                    // Simulation:
                    // Propagating the exception represents fail-fast startup behavior:
                    // - application stops instead of running in invalid state
                    // - failure is immediately visible
                    //
                    // In real applications, this would terminate startup,
                    // not return an HTTP response.
                    return Results.Problem(
                        "Startup dependency initialization failed.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            });

        return endpoints;
    }

    private sealed record StartupResult(bool Initialized, bool FailureVisible);
}