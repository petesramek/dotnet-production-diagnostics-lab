using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DiagnosticsLab.Api.Tests;

public sealed class DiagnosticsLabWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configuration = new(StringComparer.OrdinalIgnoreCase);

    public DiagnosticsLabWebApplicationFactory WithConfiguration(string key, string? value)
    {
        _configuration[key] = value;
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(_configuration);
        });
    }
}
