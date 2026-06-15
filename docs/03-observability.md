## Scenario 3: Weak observability & tracing

### Problem

POST /03-observability-tracing/problem

Processes a payment but logs only vague messages without context.

Logs do not include identifiers such as PaymentId or CustomerId, making it
difficult to trace issues in production.

---

### Improved version

POST /03-observability-tracing/improved

Uses structured logging and logging scopes with contextual information.

Logs include:
- PaymentId
- CustomerId
- Amount

This enables easier correlation and debugging.

---

### Key issue

The problem is missing contextual information in logs.

Without structured logging, production debugging becomes slow and unreliable.

---

### What to observe

- Compare log output between endpoints
- Problem endpoint produces generic logs
- Improved endpoint includes structured data and identifiers
- Failed requests can be correlated with specific inputs
