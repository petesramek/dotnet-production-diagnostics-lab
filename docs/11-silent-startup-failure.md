## Scenario 11: Silent startup failure

### Problem

GET /11-silent-startup-failure/problem

Simulates a startup-style initialization failure that is swallowed.

The application continues running:
- the failure is hidden
- the system appears healthy
- the application is actually in a broken state

This pattern is dangerous because deployments may look successful even when the system cannot operate correctly.

---

### Improved version

GET /11-silent-startup-failure/improved

Reports the failure clearly and returns a proper failure response.

The system:
- surfaces the error
- logs it as an error
- communicates failure using HTTP 503

In real startup code, this corresponds to failing fast instead of hiding initialization problems.

---

### Key issue

The problem is swallowing critical startup failures and continuing execution.

---

### What to observe

- The problem endpoint returns success even though initialization failed
- The improved endpoint returns HTTP 503
- The improved endpoint logs the failure clearly
