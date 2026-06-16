# Scenario 15: ThreadPool Starvation

## Goal

Show how blocking work contributes to thread-pool starvation and why asynchronous waiting keeps the application more responsive.

## Why this matters

ASP.NET Core relies on thread-pool threads to execute requests. If those threads are blocked for too long, new requests wait in the queue. Under load, latency rises quickly and the application can appear stalled even though CPU usage may not be fully saturated.

## Problem

**Endpoint**: `GET /15-threadpool-starvation/problem?delayMs=500`

The problem endpoint blocks the current thread while simulating work. This means:
- a thread-pool thread is occupied for the full delay
- fewer threads remain available for incoming requests
- queueing and latency increase under concurrent load

## Mitigation

**Endpoint**: `GET /15-threadpool-starvation/mitigation?delayMs=500`

The mitigation endpoint uses asynchronous waiting. This means:
- the thread is released while the operation is waiting
- the runtime can serve other requests concurrently
- latency stays lower under load

## Simulation notes

This scenario uses `Thread.Sleep(...)` versus `Task.Delay(...)` to make the difference easy to observe. In a real system, thread-pool starvation is often caused by synchronous I/O, blocking waits, or CPU-bound work performed directly on request threads.

## How to try it

```bash
curl "http://localhost:5000/15-threadpool-starvation/problem?delayMs=500"
curl "http://localhost:5000/15-threadpool-starvation/mitigation?delayMs=500"
```

Then repeat under concurrent load.

## What to observe

- The problem endpoint reports `blocking = true`.
- The mitigation endpoint reports `blocking = false`.
- Under load, the problem path increases latency more quickly.
- The mitigation path keeps the server more responsive.

## Tools to use

Use external tooling to make ThreadPool starvation visible.

Suggested tools:
- `dotnet-counters`
- optional `dotnet-trace`
- optional `dotnet-stack`
- a load tool (`wrk` or `bombardier`)

See [tools README](tools/README.md).

## Diagnostic tools

Use these tools to observe the difference:
- `wrk` → generate concurrent requests and expose queueing behavior
- `dotnet-counters` → watch thread-pool counters and runtime pressure
- `dotnet-trace` → capture scheduling and runtime activity if deeper investigation is needed

Example:

```bash
wrk -t4 -c50 -d30s "http://localhost:5000/15-threadpool-starvation/problem?delayMs=500"
wrk -t4 -c50 -d30s "http://localhost:5000/15-threadpool-starvation/mitigation?delayMs=500"
```

## Source files

- Endpoint: [../src/ProductionDiagnosticsLab.Api/Endpoints/ThreadPoolStarvationEndpoints.cs](../src/ProductionDiagnosticsLab.Api/Endpoints/ThreadPoolStarvationEndpoints.cs)
- Tests: [../tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs](../tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs)
- Smoke tests: [../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs](../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs)

## Related scenarios

- [Scenario 06: Blocking Request Handling](06-blocking-request-handling.md)
- [Scenario 02: Missing Cancellation and Timeout Handling](02-cancellation.md)

## External references

- [Debug ThreadPool starvation - .NET](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/debug-threadpool-starvation)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-10.0)
- [dotnet-counters diagnostic tool](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
