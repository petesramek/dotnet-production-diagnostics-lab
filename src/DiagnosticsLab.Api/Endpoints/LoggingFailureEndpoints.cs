using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for logging failure scenarios.
/// </summary>
public static class LoggingFailureEndpoints {
    private const string Route = "/12-logging-failure";

    public static IEndpointRouteBuilder MapLoggingFailureEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Failure in a non-critical component (logging / audit) propagates
        // and breaks the main business operation.
        group.MapPost("/problem", async (
            AuditRequest request,
            FakeAuditSink auditSink,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("LoggingFailure.Problem");

                logger.LogWarning("Executing operation with tightly coupled audit logging");

                // Simulation:
                // FakeAuditSink represents external or secondary systems, such as:
                // - logging providers (e.g. external log service)
                // - audit/event pipelines
                // - telemetry or compliance sinks
                //
                // These systems are NOT part of the core business logic.
                // However, their failure here propagates and breaks the request.
                await auditSink.WriteAsync(
                    "BusinessOperationCompleted",
                    request.AuditShouldFail,
                    cancellationToken);

                // Because we do not isolate this call:
                // - any exception from the audit sink will fail the entire request
                return Results.Ok(new AuditResult(
                    request.OperationId,
                    BusinessOperationCompleted: true,
                    AuditFailureIsolated: false));
            });

        // Mitigation:
        // Isolate failures of non-critical components so they do not affect
        // the main business operation.
        group.MapPost("/mitigation", async (
            AuditRequest request,
            FakeAuditSink auditSink,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) => {
                var logger = loggerFactory.CreateLogger("LoggingFailure.Mitigation");

                logger.LogInformation("Executing operation with isolated audit logging");

                try {
                    // Simulation:
                    // FakeAuditSink again represents secondary systems such as:
                    // - logging infrastructure
                    // - audit systems
                    // - telemetry pipelines
                    //
                    // These systems may fail independently of the core operation.
                    // By wrapping the call, we prevent those failures from propagating.
                    await auditSink.WriteAsync(
                        "BusinessOperationCompleted",
                        request.AuditShouldFail,
                        cancellationToken);
                } catch (AuditSinkException exception) {
                    // Failure is captured and handled locally instead of propagating.
                    //
                    // Because failure is isolated:
                    // - business operation succeeds
                    // - system remains resilient under partial failures
                    logger.LogError(exception,
                        "Audit sink failed but business operation continues");
                }

                return Results.Ok(new AuditResult(
                    request.OperationId,
                    BusinessOperationCompleted: true,
                    AuditFailureIsolated: true));
            });

        return endpoints;
    }

    private sealed record AuditRequest(Guid OperationId, bool AuditShouldFail);

    private sealed record AuditResult(
        Guid OperationId,
        bool BusinessOperationCompleted,
        bool AuditFailureIsolated);
}
