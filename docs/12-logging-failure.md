## Scenario 12: Logging failure

### Problem

POST /12-logging-failure/problem

The business operation depends directly on a logging or audit sink.

If the logging sink fails:
- the request fails
- the business operation is disrupted
- users experience errors caused by non-critical components

---

### Improved version

POST /12-logging-failure/improved

Handles logging failures gracefully.

If logging fails:
- the failure is logged
- the business operation still succeeds
- system stability is preserved

---

### Key issue

The problem is allowing non-critical logging failures to break core business logic.

---

### What to observe

- Problem endpoint fails when logging fails
- Improved endpoint succeeds even when logging fails
- Improved endpoint isolates side-effect failures from business logic
