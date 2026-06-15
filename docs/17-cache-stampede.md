## Scenario 17: Cache stampede

### Problem

GET /17-cache-stampede/problem

Multiple concurrent requests hit an empty cache key.

All requests recompute the value:
- duplicate expensive work
- increased latency
- downstream system overload

---

### Improved version

GET /17-cache-stampede/improved

Uses coordination (SemaphoreSlim) to ensure only one request computes the value.

Other requests:
- wait for computation
- reuse cached result

---

### Key issue

The problem is lack of coordination when cache entries expire.

---

### What to observe

Run concurrent load:

wrk -t4 -c50 http://localhost:5000/17-cache-stampede/problem

Observe logs:
- many "recomputing value"

Then run:

wrk -t4 -c50 http://localhost:5000/17-cache-stampede/improved

Observe:
- only one recomputation
- stable behavior

---

### Diagnostic signals

- log duplication vs single computation
- latency spikes in problem version
- stable latency in improved version
