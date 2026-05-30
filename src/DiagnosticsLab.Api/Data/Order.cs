namespace DiagnosticsLab.Api.Data;

/// <summary>
/// Represents an order used by the data access diagnostics scenarios.
/// </summary>
public sealed class Order
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer that owns the order.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the order total.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets the order status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
