## Scenario 7: Unbounded retries (retry storms)

### Problem

GET /07-unbounded-retries/problem?sku=FAIL

Retries immediately when the inventory provider fails.

This creates a retry storm:
- repeated rapid calls to a failing dependency
- increased pressure on an already degraded service
- risk of cascading failures across the system

---

### Improved version

GET /07-unbounded-retries/improved?sku=FAIL

Uses controlled retry behavior:
- limited number of attempts
- timeout awareness
- incremental backoff delays between retries

This reduces pressure on the dependency and fails in a predictable way.

---

### Key issue

The problem is unbounded and immediate retries without delay or limits.

---

### What to observe

- Problem endpoint retries aggressively
- Improved endpoint performs fewer attempts with delays
- Improved endpoint fails in a controlled and predictable manner
- Compare logs and number of attempts
``
