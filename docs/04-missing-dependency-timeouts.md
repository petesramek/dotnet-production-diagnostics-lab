# Scenario 04: Missing Dependency Timeouts

## Goal

Show why an application must put time boundaries around calls to external systems that it does not control.

## Why this matters

External dependencies can be slow, overloaded, or temporarily unavailable. If an application waits indefinitely, threads, connections, and request capacity remain occupied for too long. Under load, this can trigger cascading failures across the system.

## Problem

**Endpoint**: `GET /04-missing-dependency-timeouts/problem?country=SLOW`

The problem endpoint calls an external dependency without timeout control. This means:
- the request can wait indefinitely for a slow upstream service
- connections and request-processing capacity remain occupied
- one degraded dependency can affect the whole application

## Mitigation

**Endpoint**: `GET /04-missing-dependency-timeouts/mitigation?country=SLOW`

The mitigation endpoint applies timeout and cancellation. This means:
- the application stops waiting after a defined limit
- resources are released earlier
- failure is surfaced in a controlled way with HTTP `504 Gateway Timeout`

## Simulation notes

This scenario uses a fake shipping client to simulate an external system. The application does not control how fast that dependency responds. In a real system, this would typically be an outbound HTTP or gRPC call.

## How to try it

Use a normal country value and a slow value:

```bash
curl "http://localhost:5000/04-missing-dependency-timeouts/problem?country=CZ"
curl "http://localhost:5000/04-missing-dependency-timeouts/problem?country=SLOW"

curl "http://localhost:5000/04-missing-dependency-timeouts/mitigation?country=CZ"
curl "http://localhost:5000/04-missing-dependency-timeouts/mitigation?country=SLOW"
```

## What to observe

- With `country=CZ`, both endpoints should succeed.
- With `country=SLOW`, the problem endpoint waits too long.
- With `country=SLOW`, the mitigation endpoint fails fast with HTTP `504`.
- Logs should clearly show the timeout boundary in the mitigation path.

## Tools to use

Use external tooling to make the slow dependency behavior visible under load.

Suggested tools:
- `dotnet-counters`
- a load tool (`wrk` or `bombardier`)

See [tools README](tools/README.md).

## Diagnostic tools

Use these tools to observe the behavior:
- application logs → request start, timeout, and failure visibility
- `wrk` or another load generator → amplify the impact of slow upstream dependencies
- `dotnet-counters` → watch request pressure, thread-pool activity, and runtime counters under load

Example:

```bash
wrk -t4 -c20 -d30s "http://localhost:5000/04-missing-dependency-timeouts/problem?country=SLOW"
wrk -t4 -c20 -d30s "http://localhost:5000/04-missing-dependency-timeouts/mitigation?country=SLOW"
```

## Source files

- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/MissingDependencyTimeoutsEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/MissingDependencyTimeoutsEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs)

## Related scenarios

- [Scenario 02: Missing Cancellation Propagation](02-missing-cancellation-propagation.md)
- [Scenario 07: Unbounded Retries](07-unbounded-retries.md)
- [Scenario 14: Socket Exhaustion](14-socket-exhaustion.md)

## External references

- [Use the IHttpClientFactory - .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Build resilient HTTP apps: key development patterns - .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [HttpClient guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
