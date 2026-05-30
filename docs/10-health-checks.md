# Scenario 10: Health checks

## Improved version

The application exposes basic health endpoints:

- `/health/live`
- `/health/ready`

Health checks make it easier for operators, deployment tools, and hosting platforms to understand whether the process is running and ready to receive traffic.

## What to observe

- Call `/health/live` to verify the process is alive.
- Call `/health/ready` to verify the application is ready.
- Extend readiness checks with real dependencies only when those dependencies are required to serve traffic.
