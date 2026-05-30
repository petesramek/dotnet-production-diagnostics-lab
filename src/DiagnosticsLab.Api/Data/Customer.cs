namespace DiagnosticsLab.Api.Data;

/// <summary>
/// Represents a customer used by the diagnostics scenarios.
/// </summary>
public sealed class Customer
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer segment used by sample data.
    /// </summary>
    public string Segment { get; set; } = string.Empty;
}
