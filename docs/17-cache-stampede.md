# Scenario 17: Cache Stampede

## Goal

Show why concurrent cache misses need coordination and how a single-flight style mitigation prevents duplicate expensive work.

## Why this matters

When a hot cache key expires, many requests can miss at the same time. If every request recomputes the same value, latency increases, backends receive unnecessary pressure, and the application can cause a self-inflicted spike.

## Problem

**Endpoint**: `GET /17-cache-stampede/problem`

The problem endpoint recomputes a missing cache value without coordination. This means:
- multiple callers can compute the same value at once
- duplicate expensive work is performed
- downstream systems receive avoidable load
- latency spikes when a hot cache entry expires

## Mitigation

**Endpoint**: `GET /17-cache-stampede/mitigation`

The mitigation endpoint coordinates recomputation with `SemaphoreSlim`. This means:
- only one caller computes the missing value
- other callers wait and reuse the result
- backend pressure stays lower
- behavior is more stable under concurrency

## Simulation notes

This scenario uses an in-memory cache and a simulated expensive operation. In a real system, the expensive work would typically be a database query, remote API call, or another costly computation behind a cache entry.

## How to try it

Warm and call the endpoints repeatedly:

```bash
curl "http://localhost:5000/17-cache-stampede/problem"
curl "http://localhost:5000/17-cache-stampede/mitigation"
```

Then use concurrent load to make the difference visible.

## What to observe

- Both endpoints return a cached value.
- The problem endpoint reports `coordinated = false`.
- The mitigation endpoint reports `coordinated = true`.
- Under concurrent load, the problem path performs duplicate recomputation while the mitigation path performs only one recompute.

## Tools to use

Use external tooling only when you want to make concurrent cache misses visible.

Suggested tools:
- a load tool (`wrk` or `bombardier`)
- logs

See [tools README](tools/README.md).

## Diagnostic tools

Use these tools to observe the difference:
- application logs → confirm duplicate recomputation vs single recomputation
- `wrk` or another load generator → create concurrent misses on the same cache key
- `dotnet-counters` → observe runtime pressure if you want to correlate stampede behavior with allocation or throughput changes

Example:

```bash
wrk -t4 -c50 http://localhost:5000/17-cache-stampede/problem
wrk -t4 -c50 http://localhost:5000/17-cache-stampede/mitigation
```

## Source files

- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/CacheStampedeEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/CacheStampedeEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs)
- Smoke tests: [../tests/DiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs](../tests/DiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs)

## Related scenarios

- [Scenario 07: Unbounded Retries](07-unbounded-retries.md)
- [Scenario 14: Socket Exhaustion](14-socket-exhaustion.md)

## External references

- [Cache in-memory in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-10.0)
- [Overview of caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview?view=aspnetcore-10.0)
- [Caching in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching)
