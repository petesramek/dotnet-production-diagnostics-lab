namespace DiagnosticsLab.Api.Data;

public sealed class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Segment { get; set; } = string.Empty;
}
