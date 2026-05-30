using System.ComponentModel.DataAnnotations;

namespace DiagnosticsLab.Api.Configuration;

/// <summary>
/// Represents external service configuration required by the application.
/// </summary>
public sealed class ExternalServicesOptions
{
    /// <summary>
    /// The configuration section name for external service settings.
    /// </summary>
    public const string SectionName = "ExternalServices";

    /// <summary>
    /// Gets the base URL of the simulated billing API.
    /// </summary>
    [Required]
    public string BillingApiBaseUrl { get; init; } = string.Empty;
}
