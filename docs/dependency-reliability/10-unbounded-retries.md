# Scenario 10: Unbounded Retries

## Goal

Show how immediate retries against a failing dependency amplify pressure and make an outage worse.

## Why this matters

Retries are useful only when they are controlled. If every request retries immediately, a dependency that is already failing receives even more traffic. This can increase latency, prolong outages, and trigger cascading failures in the rest of the system.

## Problem

**Endpoint**: `GET /10-unbounded-retries/problem?sku=FAIL`

The problem endpoint retries immediately when the inventory provider fails. This means:
- repeated rapid calls hit the same failing dependency
- no backoff gives the dependency no time to recover
- many callers can synchronize into a retry storm

## Mitigation

**Endpoint**: `GET /10-unbounded-retries/mitigation?sku=FAIL`

The mitigation endpoint uses controlled retry behavior. This means:
- retries are limited
- timeouts bound total execution time
- backoff delays reduce pressure between attempts
- failure becomes predictable and easier to operate

## Framework alternative

The mitigation above is hand-written for teaching. .NET ships this as `Microsoft.Extensions.Http.Resilience` (built on Polly v8). For outbound `HttpClient` calls, use `AddStandardResilienceHandler()`, which bundles rate limiter, total timeout, retry with exponential backoff and jitter, circuit breaker and attempt timeout. A comment in the mitigation endpoint describes this. Jitter avoids synchronized retry spikes, which the hand-written version does not do.

## Simulation notes
This scenario uses a fake inventory client to simulate a failing external dependency. The behavior represents outbound calls to a remote API or service. The important difference is not the fake client itself, but the retry policy around it.

## How to try it

```bash
curl "http://localhost:5000/10-unbounded-retries/problem?sku=FAIL"
curl "http://localhost:5000/10-unbounded-retries/mitigation?sku=FAIL"
```

Also try a non-failing SKU:

```bash
curl "http://localhost:5000/10-unbounded-retries/problem?sku=ABC"
curl "http://localhost:5000/10-unbounded-retries/mitigation?sku=ABC"
```

## What to observe
- With a healthy SKU, both endpoints should succeed.
- With `sku=FAIL`, the problem endpoint performs aggressive retries.
- With `sku=FAIL`, the mitigation endpoint performs fewer attempts with delays.
- Logs should show the difference in retry count and timing.

## Tools to use

Use external tooling to amplify retry behavior and observe pressure.

Suggested tools:
- a load tool (`wrk` or `bombardier`)
- logs
- optional `dotnet-counters`

See [tools README](../tools/README.md).

## Diagnostic tools

Use these tools to observe the retry behavior:
- application logs → confirm retry count and timing
- `wrk` or another load generator → show how retries amplify load
- `dotnet-counters` → observe runtime pressure under repeated failing requests

Example:

```bash
wrk -t4 -c20 -d30s "http://localhost:5000/10-unbounded-retries/problem?sku=FAIL"
wrk -t4 -c20 -d30s "http://localhost:5000/10-unbounded-retries/mitigation?sku=FAIL"
```

## Source files

- Endpoint: [../../src/DiagnosticsLab.Api/Scenarios/DependencyReliability/UnboundedRetriesEndpoints.cs](../../src/DiagnosticsLab.Api/Scenarios/DependencyReliability/UnboundedRetriesEndpoints.cs)
- Tests: [../../tests/DiagnosticsLab.Api.Tests/Scenarios/DependencyReliability/DependencyReliabilityScenarioTests.cs](../../tests/DiagnosticsLab.Api.Tests/Scenarios/DependencyReliability/DependencyReliabilityScenarioTests.cs)

## Related scenarios

- [Scenario 09: Missing Dependency Timeouts](09-missing-dependency-timeouts.md)
- [Scenario 11: Socket Exhaustion](11-socket-exhaustion.md)

## External references

- [Introduction to resilient app development - .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- [Build resilient HTTP apps: key development patterns - .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Recommendations for handling transient faults](https://learn.microsoft.com/en-us/azure/well-architected/design-guides/handle-transient-faults)
