using DiagnosticsLab.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DiagnosticsLab.Api.Tests.Infrastructure;

/// <summary>
/// Test application factory that isolates each test host with its own SQLite database file.
/// </summary>
public sealed class DiagnosticsLabWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configuration = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"diagnostics-lab-tests-{Guid.NewGuid():N}.db");

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
            var testConfiguration = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["ExternalServices:BillingApiBaseUrl"] = "https://billing.example.local"
            };

            foreach (var pair in _configuration)
            {
                testConfiguration[pair.Key] = pair.Value;
            }

            configurationBuilder.AddInMemoryCollection(testConfiguration);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_databasePath}");
            });
        });
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        TryDeleteDatabaseFile(_databasePath);
        TryDeleteDatabaseFile($"{_databasePath}-shm");
        TryDeleteDatabaseFile($"{_databasePath}-wal");
    }

    private static void TryDeleteDatabaseFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup. The OS can keep SQLite files briefly locked after test disposal.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup. Test isolation does not depend on deleting an already unique file.
        }
    }
}
