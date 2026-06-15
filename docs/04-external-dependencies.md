## Scenario 4: External dependency reliability

### Problem

GET /04-external-dependency-reliability/problem?country=SLOW

Calls an external dependency without any timeout control.

If the dependency is slow, the request waits indefinitely.

This can cause:
- slow responses
- resource exhaustion
- cascading failures across services

---

### Improved version

GET /04-external-dependency-reliability/improved?country=SLOW

Uses a timeout and returns a controlled response when the dependency is too slow.

If the dependency does not respond within the expected time:
- the request is cancelled
- a 504 Gateway Timeout response is returned

---

### Key issue

The problem is unbounded waiting on external dependencies.

---

### What to observe

- Call the endpoints with country=SLOW
- Problem endpoint takes a long time
- Improved endpoint fails fast with HTTP 504
- Compare logs and response behavior
