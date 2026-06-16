# Scenario 01: Slow Data Access

## Goal
Demonstrate the difference between loading too much data into application memory and pushing filtering to the database engine.

## Why this matters
When an API loads all rows first and filters afterward, the database sends more data than necessary and the application performs work it should not do. As data grows, latency, memory usage, and database load all increase.

## Problem
**Endpoint**: `GET /01-slow-data-access/orders/problem?customerId=42`

The problem endpoint calls `ToListAsync()` before filtering. This means:
- all matching table rows are materialized in application memory first
- filtering happens in-process instead of in SQL
- unnecessary rows are transferred from the database
- memory usage and response time grow with dataset size

## Mitigation
**Endpoint**: `GET /01-slow-data-access/orders/mitigation?customerId=42`

The mitigation endpoint pushes filtering, ordering, projection, and limiting into the database query. This means:
- only the required rows are returned
- less data is transferred over the database connection
- the application allocates less memory
- performance remains stable as the dataset grows

## Simulation notes
This scenario uses a simple SQLite-backed lab database, but it represents the same production mistake seen with any relational database: loading too much data and filtering too late.

## How to try it
```bash
curl "http://localhost:5000/01-slow-data-access/orders/problem?customerId=42"
curl "http://localhost:5000/01-slow-data-access/orders/mitigation?customerId=42"
```

Try the same requests repeatedly while increasing the dataset size or running concurrent load.

## What to observe
- Both endpoints should return the same logical order data.
- The problem endpoint does more unnecessary work.
- The mitigation endpoint keeps filtering in the database.
- Under larger datasets, the problem endpoint becomes slower and allocates more memory.

## Diagnostic tools
Use these tools to observe the difference:
- `dotnet-counters` → watch allocation rate and GC activity
- database logging / query inspection → confirm filtering location
- `wrk` or another load generator → amplify the behavior under repeated requests

Example:
```bash
dotnet-counters monitor --process-id <pid> System.Runtime
wrk -t4 -c20 -d30s "http://localhost:5000/01-slow-data-access/orders/problem?customerId=42"
wrk -t4 -c20 -d30s "http://localhost:5000/01-slow-data-access/orders/mitigation?customerId=42"
```

## Source files
- Endpoint: `src/ProductionDiagnosticsLab.Api/Endpoints/SlowDataAccessEndpoints.cs`
- Tests: `tests/ProductionDiagnosticsLab.Tests/Scenarios/DataAccessScenarioTests.cs`

## Related scenarios
- Scenario 05: N+1 / Chatty Data Access
- Scenario 08: Large Response Allocation / Streaming

## External references
External references are intentionally not added in this pass because they should be validated against trusted current sources before linking.
