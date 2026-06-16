using DiagnosticsLab.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DiagnosticsLab.Api.Tests.Scenarios;

/// <summary>
/// Contains behavior tests for data access diagnostics scenarios.
/// </summary>
/// <param name="factory">The test application factory.</param>
public sealed class DataAccessScenarioTests(DiagnosticsLabWebApplicationFactory factory) : IClassFixture<DiagnosticsLabWebApplicationFactory> {
    /// <summary>
    /// Verifies that the problem endpoint returns order identifiers for the requested customer.
    /// </summary>
    [Fact]
    public async Task ExcessiveDataMaterialization_Problem_Returns_OrderId() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/01-excessive-data-materialization/problem?customerId=42");

        JsonAssertions.GetIds(problem)
            .Should()
            .NotBeEmpty();

        problem.EnumerateArray()
            .Select(item => item.GetProperty("customerId").GetInt32())
            .Should()
            .OnlyContain(customerId => customerId == 42);
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns the same logical order identifiers as the problem endpoint.
    /// </summary>
    [Fact]
    public async Task ExcessiveDataMaterialization_Mitigation_Returns_OrderId() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/01-excessive-data-materialization/problem?customerId=42");

        var mitigation = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/01-excessive-data-materialization/mitigation?customerId=42");

        JsonAssertions.GetIds(mitigation)
            .Should()
            .Equal(JsonAssertions.GetIds(problem));
    }

    /// <summary>
    /// Verifies that the problem endpoint returns customer summaries.
    /// </summary>
    [Fact]
    public async Task NPlusOneQuery_Problem_Returns_CustomerSummary() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/05-n-plus-1-data-access/problem?take=10");

        JsonAssertions.NormalizeCustomerSummaries(problem)
            .Should()
            .NotBeEmpty();
    }

    /// <summary>
    /// Verifies that the mitigation endpoint returns the same customer summaries as the problem endpoint.
    /// </summary>
    [Fact]
    public async Task NPlusOneQuery_Mitigation_Returns_CustomerSummary() {
        using var client = factory.CreateClient();

        var problem = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/05-n-plus-1-data-access/problem?take=10");

        var mitigation = await JsonTestClient.GetJsonArrayAsync(
            client,
            "/05-n-plus-1-data-access/mitigation?take=10");

        JsonAssertions.NormalizeCustomerSummaries(mitigation)
            .Should()
            .Equal(JsonAssertions.NormalizeCustomerSummaries(problem));
    }
}