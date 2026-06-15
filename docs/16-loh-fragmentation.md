## Scenario 16: LOH fragmentation

### Problem

POST /16-loh-fragmentation/problem

Reads the entire JSON payload into memory before deserialization.

This causes:
- large object allocations (> 85 KB)
- LOH (Large Object Heap) usage
- memory fragmentation
- increased Gen 2 GC pressure

---

### Improved version

POST /16-loh-fragmentation/improved

Streams JSON directly from the request body using System.Text.Json.

This:
- avoids large intermediate buffers
- reduces LOH allocations
- improves memory efficiency

---

### Key issue

The problem is allocating large buffers when processing JSON instead of streaming.

---

### What to observe

Run load:

wrk -t4 -c20 -d30s http://localhost:5000/16-loh-fragmentation/problem

Then monitor:

dotnet-counters monitor System.Runtime

Look for:
- GC Heap Size
- LOH Size
- Gen 2 GC Count

Repeat for improved endpoint.

---

### Important note

LOH fragmentation cannot be reliably observed in integration tests.

It requires:
- sustained load
- runtime diagnostics tools
