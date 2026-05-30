using System.ComponentModel.DataAnnotations;

namespace DiagnosticsLab.Api.Configuration;

public sealed class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    [Required]
    public string BillingApiBaseUrl { get; init; } = string.Empty;
}
