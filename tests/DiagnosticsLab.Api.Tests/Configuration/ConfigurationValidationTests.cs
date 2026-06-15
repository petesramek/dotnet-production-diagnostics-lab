using System.Net;
using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Configuration;

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
    /// Verifies that the problem endpoint uses unvalidated configuration
    /// while the improved endpoint uses validated configuration.
    /// </summary>
    /// <remarks>
    /// The improved endpoint relies on configuration validated at startup,
    /// ensuring invalid configuration is not used at runtime.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Invalid_configuration_problem_and_improved_endpoints_return_expected_flags() {
        using var factory = new DiagnosticsLabWebApplicationFactory();
        
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/09-invalid-configuration/problem");

        using var improved = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/09-invalid-configuration/improved");

        problem.RootElement
            .GetProperty("validated")
            .GetBoolean()
            .Should()
            .BeFalse();

        improved.RootElement
            .GetProperty("validated")
            .GetBoolean()
            .Should()
            .BeTrue();
    }
}
