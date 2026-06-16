# Scenario 14: Socket Exhaustion

## Goal
Show why creating new `HttpClient` instances repeatedly causes connection churn and can exhaust available sockets under load.

## Why this matters
Outbound HTTP communication is common in production services. If a new `HttpClient` is created per operation, connection pools are not reused efficiently and the operating system can accumulate many sockets in `TIME_WAIT`. Under load, this leads to failed outbound calls and degraded reliability.

## Problem
**Endpoint**: `GET /14-socket-exhaustion/problem`

The problem endpoint creates a new `HttpClient` repeatedly. This means:
- connection pools are not reused effectively
- socket churn increases
- available ports can be exhausted under sustained traffic

## Mitigation
**Endpoint**: `GET /14-socket-exhaustion/mitigation`

The mitigation endpoint uses `IHttpClientFactory`. This means:
- connection management is pooled and reused
- socket churn stays lower
- outbound HTTP behavior remains more stable under load

## Simulation notes
This scenario simulates repeated outbound calls but does not create real internet traffic. The important behavior is `HttpClient` lifetime management. Real socket exhaustion is best observed under local load while monitoring OS socket states.

## How to try it
```bash
curl "http://localhost:5000/14-socket-exhaustion/problem"
curl "http://localhost:5000/14-socket-exhaustion/mitigation"
```

Then increase load with a load generator.

## What to observe
- Both endpoints return success in the lab.
- The problem endpoint reports `reusedConnections = false`.
- The mitigation endpoint reports `reusedConnections = true`.
- Under load, the problem path creates more connection churn.

## Diagnostic tools
Use these tools to observe the difference:
- `wrk` → generate concurrent requests
- `netstat` / `ss` → inspect socket states such as `TIME_WAIT`
- `dotnet-counters` → watch `System.Net.Http` and runtime activity

Example:
```bash
wrk -t4 -c50 http://localhost:5000/14-socket-exhaustion/problem
wrk -t4 -c50 http://localhost:5000/14-socket-exhaustion/mitigation
```

Then inspect sockets on the host while the load is running.

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/SocketExhaustionEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs`
- Smoke tests: `tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs`

## Related scenarios
- Scenario 04: External Dependency Reliability
- Scenario 07: Retry Storms

## External references
External references are intentionally not added in this pass because they should be validated against trusted current sources before linking.
