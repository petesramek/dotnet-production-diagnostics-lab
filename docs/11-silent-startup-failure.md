# Scenario 11: Startup Failure Handling

## Goal
Show why startup-time failures must be surfaced clearly instead of being swallowed and ignored.

## Why this matters
If critical initialization fails but the application continues running, deployments may look successful while the application is actually broken. This creates delayed runtime failures, misleading health signals, and difficult troubleshooting.

## Problem
**Endpoint**: `GET /11-silent-startup-failure/problem`

The problem endpoint simulates a startup-style initialization failure that is swallowed. This means:
- the failure is hidden from operators
- the application appears to continue normally
- the system is actually in a broken or partially initialized state

## Mitigation
**Endpoint**: `GET /11-silent-startup-failure/mitigation`

The mitigation endpoint makes the startup-style failure visible and returns a failure response. This means:
- the error is surfaced clearly
- the failure is logged as an error
- the system communicates failure using HTTP `503 Service Unavailable`

In real startup code, the correct behavior is to fail fast rather than continue in an invalid state.

## Simulation notes
This scenario simulates startup behavior inside an endpoint so that it can be observed and tested repeatedly. In a real application, the equivalent logic would run during application startup or hosted service initialization, not per request.

## How to try it
```bash
curl "http://localhost:5000/11-silent-startup-failure/problem"
curl "http://localhost:5000/11-silent-startup-failure/mitigation"
```

## What to observe
- The problem endpoint returns success even though initialization failed.
- The mitigation endpoint returns HTTP `503`.
- Logs should show the difference between swallowed failure and surfaced failure.

## Diagnostic tools
Use these tools to observe the behavior:
- application logs → confirm whether the failure is hidden or surfaced
- integration tests → verify `problem` vs `mitigation` behavior consistently
- deployment/container logs → confirm whether failure would be visible during startup

This scenario is primarily about failure visibility and lifecycle correctness.

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/SilentStartupFailureEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs`

## Related scenarios
- Scenario 09: Configuration Validation
- Scenario 10: Health Checks

## External references
External references are intentionally not added in this pass because they should be validated against trusted current sources before linking.
