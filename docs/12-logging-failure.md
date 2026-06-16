# Scenario 12: Logging and Audit Failure Isolation

## Goal

Show why failures in secondary systems such as logging and audit sinks must not break core business operations.

## Why this matters

Logging, auditing, and telemetry are important, but they are often side effects rather than the primary business action. If these secondary systems fail and the application lets the exception propagate, users experience errors caused by non-critical components.

## Problem

**Endpoint**: `POST /12-logging-failure/problem`

The problem endpoint allows a logging or audit sink failure to propagate. This means:
- the request fails because a non-critical component failed
- the core business operation is disrupted
- users experience avoidable errors

## Mitigation

**Endpoint**: `POST /12-logging-failure/mitigation`

The mitigation endpoint isolates the logging or audit failure. This means:
- the side-effect failure is captured and handled locally
- the main business operation still succeeds
- the system remains more resilient during partial failures

## Simulation notes

This scenario uses a fake audit sink to simulate a secondary dependency such as an external logging provider, audit pipeline, or telemetry sink. The important behavior is whether that failure is allowed to break the primary request.

## How to try it

Use a request payload that forces the audit sink to fail:

```bash
curl -X POST "http://localhost:5000/12-logging-failure/problem"   -H "Content-Type: application/json"   -d '{"operationId":"00000000-0000-0000-0000-000000000001","auditShouldFail":true}'

curl -X POST "http://localhost:5000/12-logging-failure/mitigation"   -H "Content-Type: application/json"   -d '{"operationId":"00000000-0000-0000-0000-000000000001","auditShouldFail":true}'
```

## What to observe

- The problem endpoint fails when the audit sink throws.
- The mitigation endpoint still succeeds when the audit sink fails.
- Logs should show that the mitigation path captures the audit failure instead of propagating it.

## Diagnostic tools

Use these tools to observe the behavior:
- application logs → confirm propagation vs isolation of the side-effect failure
- integration tests → verify the problem path fails while mitigation succeeds
- centralized logging if available → observe how the side-effect failure is recorded

## Source files

- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/LoggingFailureEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/LoggingFailureEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/ObservabilityScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/ObservabilityScenarioTests.cs)

## Related scenarios

- [Scenario 03: Missing Log Context](03-missing-log-context.md)
- [Scenario 11: Startup Failure Handling](11-silent-startup-failure.md)

## External references

- [Logging in .NET and ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0)
- [Logging in C# and .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview)
- [LoggerExtensions.BeginScope API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.beginscope?view=net-11.0-pp)
