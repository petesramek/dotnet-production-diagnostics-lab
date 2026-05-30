namespace DiagnosticsLab.Api.Data;

public sealed class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;
}
