## Scenario 2: Missing cancellation & timeouts

### Problem

GET /02-cancellation-timeouts/problem

Simulates a long-running asynchronous operation that does NOT use CancellationToken.

Even if the client disconnects or the request is cancelled, the operation continues running.

This leads to:
- unnecessary work being performed
- wasted CPU time
- reduced throughput under load

---

### Improved version

GET /02-cancellation-timeouts/improved

Uses and propagates CancellationToken into the asynchronous operation.

If the request is cancelled:
- the operation stops immediately
- no additional work is performed
- resources are released earlier

---

### Key issue

The problem is ignoring CancellationToken in asynchronous operations.

---

### What to observe

- Cancel the request from the client (or use a short timeout)
- Problem endpoint continues executing
- Improved endpoint stops immediately
- Compare logs for request start and completion behavior
