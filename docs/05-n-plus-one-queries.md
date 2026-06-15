## Scenario 5: N+1 Queries

### Problem

GET /05-n-plus-1-data-access/problem?take=25

Loads customers and then executes additional queries per customer to compute order aggregates.

This results in:
- N+1 database queries
- increasing number of database roundtrips as the number of customers grows
- degraded performance and scalability

---

### Improved version

GET /05-n-plus-1-data-access/improved?take=25

Fetches customers and their order aggregates in a single query using projection.

This keeps the operation set-based and avoids unnecessary database roundtrips.

---

### Key issue

The problem is executing database queries inside a loop.

Each additional customer introduces more database calls, which does not scale with larger datasets.

---

### What to observe

- Problem endpoint performs repeated queries per customer
- Improved endpoint executes a single set-based query
- Both endpoints return the same logical result
- Performance difference increases as the dataset grows
