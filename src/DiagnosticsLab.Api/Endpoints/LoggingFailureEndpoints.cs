using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for logging and audit sink failure scenarios.
/// </summary>
public static class LoggingFailureEndpoints {
    /// <summary>
    /// Adds logging failure diagnostics endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapLoggingFailureEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/12-logging-failure");

        group.MapPost("/problem", async (AuditRequest request, FakeAuditSink auditSink, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Audit.Problem");

            logger.LogInformation("Processing audit problem scenario for operation {OperationId}", request.OperationId);

            await auditSink.WriteAsync("BusinessOperationCompleted", request.AuditShouldFail, cancellationToken);

            return Results.Ok(new AuditResult(
                request.OperationId,
                BusinessOperationCompleted: true,
                AuditFailureIsolated: false));
        });

        group.MapPost("/improved", async (AuditRequest request, FakeAuditSink auditSink, ILoggerFactory loggerFactory, CancellationToken cancellationToken) => {
            var logger = loggerFactory.CreateLogger("Audit.Improved");

            logger.LogInformation("Processing audit improved scenario for operation {OperationId}", request.OperationId);

            try {
                await auditSink.WriteAsync("BusinessOperationCompleted", request.AuditShouldFail, cancellationToken);
            } catch (AuditSinkException exception) {
                logger.LogWarning(
                    exception,
                    "Audit sink failed for operation {OperationId}. Business operation continues.",
                    request.OperationId);
            }

            return Results.Ok(new AuditResult(
                request.OperationId,
                BusinessOperationCompleted: true,
                AuditFailureIsolated: true));
        });

        return endpoints;
    }

    private sealed record AuditRequest(Guid OperationId, bool AuditShouldFail);

    private sealed record AuditResult(Guid OperationId, bool BusinessOperationCompleted, bool AuditFailureIsolated);
}
