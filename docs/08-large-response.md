## Scenario 8: Large response allocation / streaming

### Problem

GET /08-large-response/problem?rows=1000

Builds the full export dataset in memory before returning the response.

This causes:
- high memory usage
- increased GC pressure
- reduced scalability under load

---

### Improved version

GET /08-large-response/improved?rows=1000

Streams results using IAsyncEnumerable<T>.

Data is produced incrementally without allocating the full response in memory.

---

### Key issue

The problem is allocating large in-memory collections instead of streaming the response.

---

### What to observe

- Both endpoints return identical data
- Problem endpoint allocates full list
- Improved endpoint streams data
- Differences become visible with larger datasets
``
