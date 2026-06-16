# Scenario 10: Health Checks

## Goal
Show why applications should expose standard health endpoints that infrastructure can use for liveness and readiness decisions.

## Why this matters
If an application does not expose reliable health probes, load balancers, orchestrators, and hosting platforms cannot tell whether the process is alive or whether the application is actually ready to handle traffic. This leads to bad routing decisions, slow failure detection, and unhealthy instances receiving requests.

## Problem
This scenario describes the anti-pattern of running an application without meaningful health probes. In that state:
- infrastructure cannot distinguish a healthy instance from a broken one
- unhealthy instances may continue receiving traffic
- operational failures are harder to detect automatically

A common mistake is assuming that “the process is running” means “the application is ready”.

## Mitigation
The lab exposes two standard health endpoints:
- **Liveness**: `GET /health/live`
- **Readiness**: `GET /health/ready`

These endpoints integrate with ASP.NET Core health checks and give infrastructure reliable signals for traffic management and restart decisions.

## Simulation notes
Unlike the other scenarios, this one is middleware/infrastructure based rather than implemented through a dedicated scenario endpoint class. The value of this scenario is operational visibility and orchestration correctness, not application business logic.

## How to try it
```bash
curl "http://localhost:5000/health/live"
curl "http://localhost:5000/health/ready"
```

## What to observe
- `/health/live` answers whether the process is alive.
- `/health/ready` answers whether the application is ready to serve traffic.
- These endpoints provide a standard contract for orchestrators, load balancers, and deployment platforms.

## Diagnostic tools
Use these tools to validate the behavior:
- `curl` → verify probe responses manually
- container orchestration platform probes → validate liveness/readiness integration
- application logs → confirm startup and readiness transitions if present

This scenario is less about performance counters and more about reliable platform signaling.

## Source files
- Host setup: `src/ProductionDiagnosticsLab.Api/Program.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs`

## Related scenarios
- Scenario 09: Configuration Validation
- Scenario 11: Startup Failure Handling

## External references
- [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)
- [Health monitoring - .NET microservices architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/monitor-app-health)
- [Health probes in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/health-probes)
