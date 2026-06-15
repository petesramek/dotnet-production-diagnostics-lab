namespace DiagnosticsLab.Api.Endpoints;

public static class SocketExhaustionEndpoints {
    public static IEndpointRouteBuilder MapSocketExhaustionEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/14-socket-exhaustion");

        group.MapGet("/problem", async (
            IHttpContextAccessor accessor,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SocketExhaustion.Problem");

                var request = accessor.HttpContext!.Request;
                var baseUri = new Uri($"{request.Scheme}://{request.Host}");

                const int requests = 5; // keep small for stability

                for (int i = 0; i < requests; i++) {
                    using var client = new HttpClient();

                    // We are simulating HTTP call to external dependency e.g. await client.GetAync(...);
                    await Task.Delay(10, cancellationToken);
                }

                return Results.Ok(new {
                    Requests = requests,
                    ReusedConnections = false
                });
            });

        group.MapGet("/improved", async (
            IHttpClientFactory factory,
            IHttpContextAccessor accessor,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("SocketExhaustion.Improved");

                var request = accessor.HttpContext!.Request;
                var baseUri = new Uri($"{request.Scheme}://{request.Host}");

                const int requests = 5;

                var client = factory.CreateClient();

                for (int i = 0; i < requests; i++) {
                    // We are simulating HTTP call to external dependency e.g. await client.GetAync(...);
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