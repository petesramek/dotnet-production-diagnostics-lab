# Scenario 09: Configuration Validation

## Goal

Show why configuration should be validated at startup instead of being discovered as invalid during request handling.

## Why this matters

If configuration is consumed without validation, the application can start successfully but fail later in production when a request touches the invalid setting. This delays failure, makes deployments look successful, and turns configuration mistakes into runtime incidents.

## Problem

**Endpoint**: `GET /09-invalid-configuration/problem`

The problem endpoint reads raw configuration at runtime without validation. This means:
- the app may start even when required configuration is missing or invalid
- the failure appears only when the endpoint is called
- the system can run in a broken state before anyone notices

## Mitigation

**Endpoint**: `GET /09-invalid-configuration/mitigation`

The mitigation path uses strongly typed options validated at startup. This means:
- invalid configuration is detected before the application starts serving traffic
- operators get fast feedback during deployment
- runtime failures caused by missing configuration are prevented

## Simulation notes

This scenario uses application configuration for an external billing service URL. In a real system, the same issue applies to connection strings, API base URLs, credentials, feature flags, or any other required configuration that must be valid before the application can operate correctly.

## How to try it

Call the endpoints with valid configuration first:

```bash
curl "http://localhost:5000/09-invalid-configuration/problem"
curl "http://localhost:5000/09-invalid-configuration/mitigation"
```

Then break the configuration (for example `ExternalServices:BillingApiBaseUrl`) and compare:
- the problem path fails only when called
- the mitigation setup fails during application startup

## What to observe

- With valid configuration, both paths expose the configured value.
- The problem endpoint reports `validated = false`.
- The mitigation endpoint reports `validated = true`.
- With broken configuration, the mitigation approach prevents startup rather than allowing late runtime failure.

## Diagnostic tools

Use these tools to observe the behavior:
- application startup logs → confirm validation failure timing
- integration tests → verify startup failure vs runtime endpoint failure
- deployment/container logs → confirm that bad configuration is surfaced early

This scenario is mostly about failure timing and operational visibility, not performance.

## Source files

- Endpoint: [../src/ProductionDiagnosticsLab.Api/Endpoints/InvalidConfigurationEndpoints.cs](../src/ProductionDiagnosticsLab.Api/Endpoints/InvalidConfigurationEndpoints.cs)
- Tests: [../tests/ProductionDiagnosticsLab.Tests/Scenarios/ConfigurationTests.cs](../tests/ProductionDiagnosticsLab.Tests/Scenarios/ConfigurationTests.cs)

## Related scenarios

- [Scenario 10: Health Checks](10-health-checks.md)
- [Scenario 11: Startup Failure Handling](11-silent-startup-failure.md)

## External references

- [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0)
- [ValidateOnStart API reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.optionsbuilderextensions.validateonstart?view=net-11.0-pp)
- [Options pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
