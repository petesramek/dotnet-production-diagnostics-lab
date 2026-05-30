using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

/// <summary>
/// Contains tests for startup configuration validation and validated configuration endpoints.
/// </summary>
public sealed class ConfigurationValidationTests
{
    /// <summary>
    /// Verifies that the application fails fast when required external service configuration is missing.
    /// </summary>
    [Fact]
    public void Application_fails_fast_when_required_configuration_is_missing()
    {
        using var factory = new DiagnosticsLabWebApplicationFactory()
            .WithConfiguration("ExternalServices:BillingApiBaseUrl", string.Empty);

        var act = () => factory.CreateClient();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*BillingApiBaseUrl*");
    }

    /// <summary>
    /// Verifies that the improved configuration endpoint returns configuration validated at startup.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Improved_configuration_endpoint_returns_validated_configuration()
    {
        using var factory = new DiagnosticsLabWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/config/improved");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
