using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DiagnosticsLab.Api.Tests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void Application_fails_fast_when_required_configuration_is_missing()
    {
        using var factory = new DiagnosticsLabWebApplicationFactory()
            .WithConfiguration("ExternalServices:BillingApiBaseUrl", string.Empty);

        var act = () => factory.CreateClient();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*BillingApiBaseUrl*");
    }

    [Fact]
    public async Task Improved_configuration_endpoint_returns_validated_configuration()
    {
        using var factory = new DiagnosticsLabWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/config/improved");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
