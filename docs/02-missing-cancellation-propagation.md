# Scenario 02: Missing Cancellation Propagation

## Goal

Demonstrate why asynchronous operations must receive and honor `CancellationToken` when request processing is cancelled.

## Why this matters

If a request is aborted but the server continues executing database calls, HTTP calls, or other asynchronous work, the system wastes resources on work whose result is no longer needed. Under load, this reduces throughput and increases pressure on dependent systems.

## Problem

**Endpoint**: `GET /02-missing-cancellation-propagation/problem`

The problem endpoint starts asynchronous work without passing `CancellationToken`. This means:
- the operation keeps running even if the client disconnects
- server resources stay busy longer than necessary
- unnecessary work is performed under load

## Mitigation

**Endpoint**: `GET /02-missing-cancellation-propagation/mitigation`

The mitigation endpoint propagates `CancellationToken` into the asynchronous operation. This means:
- the operation can stop immediately when the request is cancelled
- remaining work is skipped
- resources are released earlier

## Simulation notes

This scenario uses `Task.Delay(...)` to simulate asynchronous work. In a real system, the same rule applies to operations such as:
- `dbContext.Entities.ToListAsync(cancellationToken)`
- `httpClient.SendAsync(request, cancellationToken)`
- streamed I/O or other cancellable async APIs

## How to try it

```bash
curl "http://localhost:5000/02-missing-cancellation-propagation/problem"
curl "http://localhost:5000/02-missing-cancellation-propagation/mitigation"
```

To observe the difference clearly, call the endpoint with a client timeout or cancel the request before it completes.

## What to observe

- The problem endpoint continues without cancellation support.
- The mitigation endpoint is cancellation-aware.
- Logs should make the behavior difference visible.
- Under load, missing cancellation wastes capacity.

## Diagnostic tools

Use these tools to observe the behavior:
- application logs → confirm request started vs cancelled work
- `dotnet-counters` → watch active work, allocation rate, and thread-pool behavior under load
- `curl --max-time` or an HTTP client timeout → trigger earlier cancellation

Example:

```bash
curl --max-time 1 "http://localhost:5000/02-missing-cancellation-propagation/problem"
curl --max-time 1 "http://localhost:5000/02-missing-cancellation-propagation/mitigation"
```

## Source files

- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/MissingCancellationPropagationEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/MissingCancellationPropagationEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs)

## Related scenarios

- [Scenario 04: Missing Dependency Timeouts](04-external-dependencies.md)
- [Scenario 06: Blocking Request Handling](06-blocking-request-handling.md)

## External references

- [Cancellation in Managed Threads - .NET](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Task cancellation - .NET](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation)
- [Cancel async tasks after a period of time - C#](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/cancel-async-tasks-after-a-period-of-time)
