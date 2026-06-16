# Scenario 08: Large Response Buffering vs Streaming

## Goal
Show why buffering an entire response in memory is more expensive than producing the response incrementally.

## Why this matters
When an API builds a large result set in memory before returning it, memory usage rises, garbage collection pressure increases, and scalability drops. Streaming keeps memory usage more stable because data is produced gradually instead of all at once.

## Problem
**Endpoint**: `GET /08-large-response/problem?rows=1000`

The problem endpoint materializes the full result set before sending the response. This means:
- a large in-memory collection is allocated up front
- more memory is retained until serialization completes
- garbage collection pressure increases as the dataset grows

## Mitigation
**Endpoint**: `GET /08-large-response/mitigation?rows=1000`

The mitigation endpoint streams the response using `IAsyncEnumerable`. This means:
- data is produced incrementally
- the full result set is not buffered in memory first
- memory usage remains lower and more stable

## Simulation notes
This scenario uses generated rows to keep the behavior deterministic. In a real system, the same tradeoff appears when returning large database exports, feeds, or report payloads.

## How to try it
```bash
curl "http://localhost:5000/08-large-response/problem?rows=1000"
curl "http://localhost:5000/08-large-response/mitigation?rows=1000"
```

Increase `rows` to make the difference more visible.

## What to observe
- Both endpoints should return the same logical data.
- The problem endpoint allocates the full list before returning it.
- The mitigation endpoint streams results incrementally.
- Memory pressure becomes more visible as response size grows.

## Diagnostic tools
Use these tools to observe the difference:
- `dotnet-counters` → watch allocation rate, GC collections, and heap growth
- `wrk` or another load generator → amplify the effect under repeated requests
- application logs if you want to correlate request size and response behavior

Example:
```bash
dotnet-counters monitor --process-id <pid> System.Runtime
wrk -t4 -c20 -d30s "http://localhost:5000/08-large-response/problem?rows=5000"
wrk -t4 -c20 -d30s "http://localhost:5000/08-large-response/mitigation?rows=5000"
```

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/LargeResponseEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs`

## Related scenarios
- Scenario 01: Slow Data Access
- Scenario 16: LOH Fragmentation

## External references
- [Controller action return types in ASP.NET Core web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-10.0)
- [System.Text.Json IAsyncEnumerable serialization](https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/6.0/iasyncenumerable-serialization)
- [MVC no longer buffers IAsyncEnumerable types when using System.Text.Json](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/6/iasyncenumerable-not-buffered-by-mvc?view=aspnetcore-10.0)
