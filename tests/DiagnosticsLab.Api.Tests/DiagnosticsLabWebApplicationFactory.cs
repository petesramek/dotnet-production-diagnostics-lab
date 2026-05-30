using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DiagnosticsLab.Api.Tests;

/// <summary>
/// Test application factory that allows scenario-specific configuration overrides.
/// </summary>
public sealed class DiagnosticsLabWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configuration = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds or replaces a configuration value for the test host.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <returns>The current factory instance.</returns>
    public DiagnosticsLabWebApplicationFactory WithConfiguration(string key, string? value)
    {
        _configuration[key] = value;
        return this;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(_configuration);
        });
    }
}
