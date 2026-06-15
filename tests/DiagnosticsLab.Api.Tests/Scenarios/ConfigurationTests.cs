using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains tests for startup configuration validation and configuration endpoints.
/// </summary>
public sealed class ConfigurationTests {
    /// <summary>
    /// Verifies that application fails fast when required configuration is missing.
    /// </summary>
    [Fact]
    public void Configuration_Problem_Throws_When_BillingApiBaseUrl_Missing() {
        using var factory = new DiagnosticsLabWebApplicationFactory()
            .WithConfiguration("ExternalServices:BillingApiBaseUrl", string.Empty);

        var act = () => factory.CreateClient();

        act.Should()
            .Throw<OptionsValidationException>()
            .WithMessage("*BillingApiBaseUrl*");
    }

    /// <summary>
    /// Verifies that problem endpoint returns unvalidated configuration.
    /// </summary>
    [Fact]
    public async Task Configuration_Problem_Return_Validated_False() {
        using var factory = new DiagnosticsLabWebApplicationFactory();
        using var client = factory.CreateClient();

        using var problem = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/09-invalid-configuration/problem");

        problem.RootElement
            .GetProperty("validated")
            .GetBoolean()
            .Should()
            .BeFalse();
    }

    /// <summary>
    /// Verifies that mitigation endpoint returns validated configuration.
    /// </summary>
    [Fact]
    public async Task Configuration_Mitigation_Return_Validated_True() {
        using var factory = new DiagnosticsLabWebApplicationFactory();
        using var client = factory.CreateClient();

        using var mitigation = await JsonTestClient.GetJsonDocumentAsync(
            client,
            "/09-invalid-configuration/mitigation");

        mitigation.RootElement
            .GetProperty("validated")
            .GetBoolean()
            .Should()
            .BeTrue();
    }
}