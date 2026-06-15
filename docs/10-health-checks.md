## Scenario 10: Missing health checks

### Problem

The application does not expose meaningful health check endpoints.

Without proper health checks:
- orchestration systems cannot determine if the application is healthy
- dead or unhealthy instances may continue receiving traffic
- failures are harder to detect and isolate

A common anti-pattern is relying on ad-hoc endpoints or assuming the application is healthy if it is running.

---

### Improved version

The application exposes standardized health check endpoints:

- /health/live → liveness probe (is the process alive)
- /health/ready → readiness probe (is the application ready to serve traffic)

These endpoints integrate with ASP.NET Core health checks infrastructure.

---

### Key issue

The problem is not providing health endpoints that infrastructure can rely on.

Without them:
- load balancers and orchestrators cannot make correct routing decisions
- failures remain hidden

---

### What to observe

- /health/live always returns process-level status
- /health/ready reflects application readiness
- These endpoints enable proper container orchestration behavior
``
