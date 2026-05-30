namespace DiagnosticsLab.Api.Endpoints;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/payments");

        group.MapPost("/problem", (PaymentRequest request, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Payments.Problem");

            if (request.Amount <= 0)
            {
                logger.LogWarning("Payment failed");
                return Results.BadRequest(new { Error = "Invalid payment" });
            }

            logger.LogInformation("Payment processed");
            return Results.Ok(new { Status = "Processed" });
        });

        group.MapPost("/observable", (PaymentRequest request, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Payments.Observable");
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["PaymentId"] = request.PaymentId,
                ["CustomerId"] = request.CustomerId
            });

            if (request.Amount <= 0)
            {
                logger.LogWarning(
                    "Payment validation failed for customer {CustomerId}. Amount {Amount} is invalid.",
                    request.CustomerId,
                    request.Amount);

                return Results.BadRequest(new
                {
                    Error = "Invalid payment amount",
                    request.PaymentId,
                    request.CustomerId
                });
            }

            logger.LogInformation(
                "Payment {PaymentId} processed for customer {CustomerId} with amount {Amount}",
                request.PaymentId,
                request.CustomerId,
                request.Amount);

            return Results.Ok(new { Status = "Processed", request.PaymentId });
        });

        return endpoints;
    }

    private sealed record PaymentRequest(Guid PaymentId, int CustomerId, decimal Amount);
}
