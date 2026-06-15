## Scenario 13: Request body memory pressure

### Problem

POST /13-request-body-memory-pressure/problem

Reads the entire request body into memory before processing it.

This causes:
- high memory usage for large uploads
- increased GC pressure
- scalability issues under concurrent load

---

### Improved version

POST /13-request-body-memory-pressure/improved

Processes the request body incrementally.

The implementation:
- streams data in chunks
- enforces a maximum size limit
- computes a SHA-256 hash without buffering the full body

---

### Key issue

The problem is buffering large request bodies instead of streaming them.

---

### What to observe

- Problem endpoint loads full body into memory
- Improved endpoint processes data incrementally
- Large uploads are rejected safely in improved endpoint
- Memory usage remains stable with streaming
