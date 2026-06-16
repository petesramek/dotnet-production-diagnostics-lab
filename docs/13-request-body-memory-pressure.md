# Scenario 13: Request Body Memory Pressure

## Goal
Show why buffering the entire request body is more expensive than processing it incrementally.

## Why this matters
Large uploads can create significant memory pressure if the application reads the whole request body into memory before doing any useful work. Under concurrent load, this increases allocation rate, garbage collection pressure, and overall memory usage.

## Problem
**Endpoint**: `POST /13-request-body-memory-pressure/problem`

The problem endpoint copies the entire request body into memory before processing it. This means:
- large uploads allocate large in-memory buffers
- garbage collection pressure increases
- concurrent uploads reduce scalability

## Mitigation
**Endpoint**: `POST /13-request-body-memory-pressure/mitigation`

The mitigation endpoint processes the request body incrementally. This means:
- data is read in chunks instead of buffered in full
- a maximum upload size can be enforced safely
- a SHA-256 hash can be computed without retaining the whole body in memory

## Simulation notes
This scenario uses a simple request body rather than a full multipart upload pipeline so the memory tradeoff stays easy to observe. In a real system, the same pattern appears in large JSON requests, file uploads, and import endpoints.

## How to try it
Send a small body first:

```bash
curl -X POST "http://localhost:5000/13-request-body-memory-pressure/problem"   -H "Content-Type: text/plain"   --data-binary "small upload body"

curl -X POST "http://localhost:5000/13-request-body-memory-pressure/mitigation"   -H "Content-Type: text/plain"   --data-binary "small upload body"
```

Then try a larger body to observe the mitigation path rejecting oversized uploads.

## What to observe
- The problem endpoint buffers the full body.
- The mitigation endpoint reports streamed processing.
- The mitigation endpoint can reject large bodies safely.
- Memory usage stays more stable when streaming is used.

## Diagnostic tools
Use these tools to observe the difference:
- `dotnet-counters` → watch allocation rate and GC activity during repeated uploads
- `wrk`, `curl`, or another request generator → send repeated or larger request bodies
- application logs → confirm size-limit rejection and streamed processing behavior

Example:
```bash
dotnet-counters monitor --process-id <pid> System.Runtime
```

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/RequestBodyMemoryPressureEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs`
- Smoke tests: `tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs`

## Related scenarios
- Scenario 08: Large Response Buffering vs Streaming
- Scenario 16: LOH Fragmentation

## External references
External references are intentionally not added in this pass because they should be validated against trusted current sources before linking.
