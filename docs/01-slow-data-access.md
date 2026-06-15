## Scenario 1: Slow data access

### Problem

GET /01-slow-data-access/orders/problem

Loads all orders into memory using ToListAsync and only then filters.

This causes:
- unnecessary database data transfer
- increased memory usage
- slower performance as data grows

---

### Improved version

GET /01-slow-data-access/orders/improved

Executes filtering in the database using Where, applies AsNoTracking, projects required fields, and limits results before materialization.

---

### Key issue

The main problem is filtering after data is materialized, preventing the database from executing the query efficiently.

---

### What to observe

- Both endpoints return the same data
- Problem endpoint becomes slower as dataset grows
- Filtering location differs: in-memory vs database
