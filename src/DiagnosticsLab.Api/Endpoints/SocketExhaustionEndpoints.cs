namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for HttpClient socket exhaustion scenarios.
/// </summary>
public static class SocketExhaustionEndpoints {
    private const string Route = "/14-socket-exhaustion";

    public static IEndpointRouteBuilder MapSocketExhaustionEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Creating a new HttpClient per operation leads to excessive socket usage
        // and can exhaust available connections under load.
        group.MapGet("/problem", async (
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SocketExhaustion.Problem");

                const int requests = 5;

                logger.LogWarning("Creating {Requests} HttpClient instances", requests);

                for (int i = 0; i < requests; i++) {
                    using var client = new HttpClient();

                    // Simulation:
                    // Each HttpClient instance may create its own connection pool.
                    // In real systems this results in:
                    // - many TCP connections being opened
                    // - connections entering TIME_WAIT after use
                    // - eventual exhaustion of available ports under load
                    //
                    // Task.Delay is used instead of real HTTP calls because
                    // socket exhaustion depends on OS-level networking,
                    // which cannot be reproduced inside this test environment.
                    await Task.Delay(10, cancellationToken);
                }

                return Results.Ok(new {
                    Requests = requests,
                    ReusedConnections = false
                });
            });

        // Mitigation:
        // Reuse HttpClient instances via IHttpClientFactory
        // so underlying connections are pooled and reused.
        group.MapGet("/mitigation", async (
            IHttpClientFactory factory,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SocketExhaustion.Mitigation");

                const int requests = 5;

                var client = factory.CreateClient();

                logger.LogInformation("Reusing HttpClient for {Requests} operations", requests);

                for (int i = 0; i < requests; i++) {
                    // Simulation:
                    // Reusing HttpClient represents connection pooling:
                    // - connections are reused instead of recreated
                    // - number of sockets remains stable
                    // - system avoids port exhaustion and connection churn
                    await Task.Delay(10, cancellationToken);
                }

                return Results.Ok(new {
                    Requests = requests,
                    ReusedConnections = true
                });
            });

        return endpoints;
    }
}