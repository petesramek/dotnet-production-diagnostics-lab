# Scenario 03: Missing Log Context

## Goal

Show the difference between vague logging and structured, contextual logging that supports real troubleshooting.

## Why this matters

An endpoint can be functionally correct and still be operationally hard to debug. If logs do not include identifiers and structured fields, failures are harder to correlate with user actions, requests, and domain events.

## Problem

**Endpoint**: `POST /03-missing-log-context/problem`

The problem endpoint processes a payment but emits generic logs without useful context. This means:
- logs do not identify the payment or customer
- failures are harder to correlate to specific requests
- troubleshooting becomes slow and unreliable

## Mitigation

**Endpoint**: `POST /03-missing-log-context/mitigation`

The mitigation endpoint uses structured logging and logging scope/context. Logs include fields such as:
- `PaymentId`
- `CustomerId`
- `Amount`

This makes the operation traceable and easier to investigate.

## Simulation notes

This scenario does not simulate performance; it simulates operational visibility. The difference is not mainly in the HTTP response, but in how much context appears in logs and how easy it is to correlate events.

## How to try it

Use invalid input to force the validation path in both endpoints:

```bash
curl -X POST "http://localhost:5000/03-missing-log-context/problem"   -H "Content-Type: application/json"   -d '{"paymentId":"00000000-0000-0000-0000-000000000001","customerId":42,"amount":-5}'

curl -X POST "http://localhost:5000/03-missing-log-context/mitigation"   -H "Content-Type: application/json"   -d '{"paymentId":"00000000-0000-0000-0000-000000000001","customerId":42,"amount":-5}'
```

## What to observe

- The problem endpoint returns a basic validation error payload.
- The mitigation endpoint returns a validation payload with additional identifiers.
- The most important difference is in the logs:
  - problem → generic message
  - mitigation → structured fields and contextual identifiers

## Diagnostic tools

Use these tools to observe the difference:
- application console logs
- centralized logging system if available
- request correlation or tracing tooling if wired into the app

The key observation for this scenario is log quality, not raw performance.

## Source files

- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/MissingLogContextEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/MissingLogContextEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/ObservabilityScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/ObservabilityScenarioTests.cs)

## Related scenarios

- [Scenario 10: Missing Health Probes](10-missing-health-probes.md)
- [Scenario 12: Logging Failure Isolation](12-logging-and-audit-failure-propagation.md)

## External references

- [Logging in .NET and ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0)
- [Logging in C# and .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview)
- [Use OpenTelemetry with OTLP and the standalone Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-otlp-example)
