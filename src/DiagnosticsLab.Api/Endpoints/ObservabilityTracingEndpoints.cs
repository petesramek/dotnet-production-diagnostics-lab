namespace DiagnosticsLab.Api.Endpoints;

/// <summary>
/// Maps endpoints for observability and tracing scenarios.
/// </summary>
public static class ObservabilityTracingEndpoints {
    private const string Route = "/03-observability-tracing";

    public static IEndpointRouteBuilder MapObservabilityTracingEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup(Route);

        // Problem:
        // Logs lack structure and context, making it difficult to trace requests,
        // diagnose issues, and correlate events across the system.
        group.MapPost("/problem", (PaymentRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("ObservabilityTracing.Problem");

            // Simulation:
            // This represents basic logging without structure or correlation.
            //
            // Real-world equivalent:
            // - simple log lines without identifiers
            // - no request or user context
            // - no way to correlate logs between operations
            //
            // This makes debugging difficult, especially in distributed systems.

            if (request.Amount <= 0) {
                // No context: which request? which user? which payment?
                logger.LogWarning("Payment failed");

                return Results.BadRequest(new {
                    Error = "Invalid payment"
                });
            }

            // Again, no context — logs cannot be correlated to a specific operation
            logger.LogInformation("Payment processed");

            return Results.Ok(new {
                Status = "Processed"
            });
        });

        // Mitigation:
        // Use structured logging and scopes to enrich logs with contextual data,
        // making them traceable and meaningful.
        group.MapPost("/mitigation", (PaymentRequest request, ILoggerFactory loggerFactory) => {
            var logger = loggerFactory.CreateLogger("ObservabilityTracing.Mitigation");

            // Simulation:
            // BeginScope represents attaching contextual data to all log entries
            // within the operation scope.
            //
            // Real-world equivalent:
            // - correlation IDs (TraceId, RequestId)
            // - user identifiers
            // - domain-specific context (PaymentId, OrderId)
            //
            // This allows logs to be grouped and analyzed as a single operation.
            using var scope = logger.BeginScope(new Dictionary<string, object> {
                ["PaymentId"] = request.PaymentId,
                ["CustomerId"] = request.CustomerId
            });

            if (request.Amount <= 0) {
                // Now logs include structured data:
                // - PaymentId
                // - CustomerId
                // making failures traceable and diagnosable
                logger.LogWarning(
                    "Payment validation failed for customer {CustomerId}. Amount {Amount} is invalid.",
                    request.CustomerId,
                    request.Amount);

                return Results.BadRequest(new {
                    Error = "Invalid payment amount",
                    request.PaymentId,
                    request.CustomerId
                });
            }

            // Structured log with parameters allows:
            // - querying logs by fields
            // - aggregating metrics
            // - correlating events
            logger.LogInformation(
                "Payment {PaymentId} processed for customer {CustomerId} with amount {Amount}",
                request.PaymentId,
                request.CustomerId,
                request.Amount);

            return Results.Ok(new {
                Status = "Processed",
                request.PaymentId
            });
        });

        return endpoints;
    }

    private sealed record PaymentRequest(Guid PaymentId, int CustomerId, decimal Amount);
}
