using DiagnosticsLab.Api.Services;

namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for audit and logging sink failure scenarios.
/// </summary>
public static class AuditEndpoints
{
    /// <summary>
    /// Adds audit diagnostics endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/audit");

        group.MapPost("/problem", async (AuditRequest request, FakeAuditSink auditSink, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Audit.Problem");
            logger.LogInformation("Processing audit problem scenario for operation {OperationId}", request.OperationId);

            await auditSink.WriteAsync("BusinessOperationCompleted", request.AuditShouldFail, cancellationToken);

            return Results.Ok(new AuditResult(request.OperationId, BusinessOperationCompleted: true, AuditFailureIsolated: false));
        });

        group.MapPost("/improved", async (AuditRequest request, FakeAuditSink auditSink, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("Audit.Improved");
            logger.LogInformation("Processing audit improved scenario for operation {OperationId}", request.OperationId);

            try
            {
                await auditSink.WriteAsync("BusinessOperationCompleted", request.AuditShouldFail, cancellationToken);
            }
            catch (AuditSinkException exception)
            {
                logger.LogWarning(exception, "Non-critical audit sink failed for operation {OperationId}; business operation remains successful", request.OperationId);
            }

            return Results.Ok(new AuditResult(request.OperationId, BusinessOperationCompleted: true, AuditFailureIsolated: true));
        });

        return endpoints;
    }

    private sealed record AuditRequest(Guid OperationId, bool AuditShouldFail);

    private sealed record AuditResult(Guid OperationId, bool BusinessOperationCompleted, bool AuditFailureIsolated);
}
