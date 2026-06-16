# Scenario 06: Blocking Request Handling

## Goal
Show the difference between blocking a request thread and using asynchronous waiting that releases the thread back to the runtime.

## Why this matters
When request handling blocks a thread with synchronous waiting, fewer threads remain available to process incoming work. Under load, this reduces throughput and can contribute to thread-pool starvation.

## Problem
**Endpoint**: `GET /06-blocking-request-handling/problem?delayMs=500`

The problem endpoint blocks the current request thread using `Thread.Sleep`. This means:
- the thread does no useful work while waiting
- the runtime cannot reuse that thread for other requests
- throughput falls under concurrent load

## Mitigation
**Endpoint**: `GET /06-blocking-request-handling/mitigation?delayMs=500`

The mitigation endpoint uses asynchronous waiting with `Task.Delay(...)`. This means:
- the request thread is released while waiting
- other requests can use the thread pool more efficiently
- the application scales better under load

## Simulation notes
This scenario uses `Thread.Sleep(...)` versus `Task.Delay(...)` to make the difference visible. In a real system, the same problem appears with synchronous I/O, blocking waits, or sync-over-async patterns in request processing.

## How to try it
```bash
curl "http://localhost:5000/06-blocking-request-handling/problem?delayMs=500"
curl "http://localhost:5000/06-blocking-request-handling/mitigation?delayMs=500"
```

Then repeat under load.

## What to observe
- Both endpoints return the same logical response shape.
- The problem endpoint reports blocking behavior.
- The mitigation endpoint reports non-blocking behavior.
- Under load, blocking reduces throughput and increases latency.

## Diagnostic tools
Use these tools to observe the behavior:
- `wrk` or another load generator → generate concurrent requests
- `dotnet-counters` → watch thread-pool activity, queue length, and runtime counters
- application logs → confirm blocking vs non-blocking path execution

Example:
```bash
wrk -t4 -c50 -d30s "http://localhost:5000/06-blocking-request-handling/problem?delayMs=500"
wrk -t4 -c50 -d30s "http://localhost:5000/06-blocking-request-handling/mitigation?delayMs=500"
```

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/BlockingRequestHandlingEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs`

## Related scenarios
- Scenario 02: Missing Cancellation and Timeout Handling
- Scenario 15: ThreadPool Starvation

## External references
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/asynchronous-programming/async-scenarios)- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-10.0)
- [Task.Delay Method](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay?view=net-10.0)
