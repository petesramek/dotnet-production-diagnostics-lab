# Scenario 16: LOH Fragmentation

## Goal

Show why buffering large JSON payloads creates Large Object Heap (LOH) pressure and why streaming deserialization is safer for memory-intensive workloads.

## Why this matters

Objects larger than roughly 85 KB are allocated on the Large Object Heap. Repeated allocation of large buffers increases GC pressure, can contribute to fragmentation, and makes memory behavior less predictable under load.

## Problem

**Endpoint**: `POST /16-loh-fragmentation/problem`

The problem endpoint buffers the full request body into memory before deserialization. This means:
- large intermediate buffers are allocated
- LOH usage increases
- Gen 2 collections become more expensive under sustained load
- memory fragmentation risk grows over time

## Mitigation

**Endpoint**: `POST /16-loh-fragmentation/mitigation`

The mitigation endpoint deserializes directly from the request stream. This means:
- large intermediate buffers are avoided
- LOH allocations are reduced
- memory usage stays more stable under load
- JSON processing becomes more efficient for large payloads

## Simulation notes

This scenario uses a generated large JSON payload so the memory behavior is reproducible in the lab. The same pattern occurs in production when APIs receive large JSON documents, imports, or bulk request payloads.

## How to try it

First generate a large payload:

```bash
curl "http://localhost:5000/16-loh-fragmentation/generate" -o payload.json
```

Then post it to both endpoints:

```bash
curl -X POST "http://localhost:5000/16-loh-fragmentation/problem"   -H "Content-Type: application/json"   --data-binary @payload.json

curl -X POST "http://localhost:5000/16-loh-fragmentation/mitigation"   -H "Content-Type: application/json"   --data-binary @payload.json
```

## What to observe

- Both endpoints should process the payload successfully.
- The problem endpoint reports `streamed = false` and `lohAllocation = true`.
- The mitigation endpoint reports `streamed = true` and `lohAllocation = false`.
- Under repeated load, the problem path should show stronger memory pressure.

## Tools to use

Use external tooling to observe LOH and GC behavior under repeated large payload processing.

Suggested tools:
- `dotnet-counters`
- optional `dotnet-gcdump`
- a load tool (`wrk` or `bombardier`)

See [tools README](tools/README.md).

## Diagnostic tools

Use these tools to observe the difference:
- `dotnet-counters` → watch GC heap size, LOH size, allocation rate, and Gen 2 collections
- `wrk` or repeated `curl` calls → sustain load long enough to make the memory pattern visible
- `dotnet-gcdump` or `dotnet-trace` → deeper inspection if you need allocation evidence

Example:

```bash
dotnet-counters monitor --process-id <pid> System.Runtime
wrk -t4 -c20 -d30s -s post-json.lua http://localhost:5000/16-loh-fragmentation/problem
wrk -t4 -c20 -d30s -s post-json.lua http://localhost:5000/16-loh-fragmentation/mitigation
```

## Source files

- Endpoint: [../src/ProductionDiagnosticsLab.Api/Endpoints/LohFragmentationEndpoints.cs](../src/ProductionDiagnosticsLab.Api/Endpoints/LohFragmentationEndpoints.cs)
- Tests: [../tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs](../tests/ProductionDiagnosticsLab.Tests/Scenarios/PerformanceScenarioTests.cs)
- Smoke tests: [../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs](../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs)

## Related scenarios

- [Scenario 08: Large Response Buffering vs Streaming](08-large-response.md)
- [Scenario 13: Request Body Memory Pressure](13-request-body-memory-pressure.md)

## External references

- [Large object heap (LOH) on Windows - .NET](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap)
- [Garbage collection and performance - .NET](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/performance)
- [Perf insights for high LOH fragmentation](https://learn.microsoft.com/en-us/visualstudio/profiling/perf-insights-high-loh-fragmentation?view=visualstudio)
