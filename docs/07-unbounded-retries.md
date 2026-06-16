# Scenario 07: Retry Storms

## Goal
Show how immediate retries against a failing dependency amplify pressure and make an outage worse.

## Why this matters
Retries are useful only when they are controlled. If every request retries immediately, a dependency that is already failing receives even more traffic. This can increase latency, prolong outages, and trigger cascading failures in the rest of the system.

## Problem
**Endpoint**: `GET /07-unbounded-retries/problem?sku=FAIL`

The problem endpoint retries immediately when the inventory provider fails. This means:
- repeated rapid calls hit the same failing dependency
- no backoff gives the dependency no time to recover
- many callers can synchronize into a retry storm

## Mitigation
**Endpoint**: `GET /07-unbounded-retries/mitigation?sku=FAIL`

The mitigation endpoint uses controlled retry behavior. This means:
- retries are limited
- timeouts bound total execution time
- backoff delays reduce pressure between attempts
- failure becomes predictable and easier to operate

## Simulation notes
This scenario uses a fake inventory client to simulate a failing external dependency. The behavior represents outbound calls to a remote API or service. The important difference is not the fake client itself, but the retry policy around it.

## How to try it
```bash
curl "http://localhost:5000/07-unbounded-retries/problem?sku=FAIL"
curl "http://localhost:5000/07-unbounded-retries/mitigation?sku=FAIL"
```

Also try a non-failing SKU:

```bash
curl "http://localhost:5000/07-unbounded-retries/problem?sku=ABC"
curl "http://localhost:5000/07-unbounded-retries/mitigation?sku=ABC"
```

## What to observe
- With a healthy SKU, both endpoints should succeed.
- With `sku=FAIL`, the problem endpoint performs aggressive retries.
- With `sku=FAIL`, the mitigation endpoint performs fewer attempts with delays.
- Logs should show the difference in retry count and timing.

## Diagnostic tools
Use these tools to observe the retry behavior:
- application logs → confirm retry count and timing
- `wrk` or another load generator → show how retries amplify load
- `dotnet-counters` → observe runtime pressure under repeated failing requests

Example:
```bash
wrk -t4 -c20 -d30s "http://localhost:5000/07-unbounded-retries/problem?sku=FAIL"
wrk -t4 -c20 -d30s "http://localhost:5000/07-unbounded-retries/mitigation?sku=FAIL"
```

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/UnboundedRetriesEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs`

## Related scenarios
- Scenario 04: External Dependency Reliability
- Scenario 14: Socket Exhaustion

## External references
External references are intentionally not added in this pass because they should be validated against trusted current sources before linking.
