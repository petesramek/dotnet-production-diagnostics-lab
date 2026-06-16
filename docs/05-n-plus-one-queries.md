# Scenario 05: N+1 / Chatty Data Access

## Goal

Demonstrate how loop-driven data access creates many unnecessary database round-trips and why set-based queries scale better.

## Why this matters

An API can return correct results and still be inefficient if it repeatedly queries the database inside a loop. As the number of parent entities grows, the number of database round-trips grows too, which increases latency and reduces scalability.

## Problem

**Endpoint**: `GET /05-n-plus-1-data-access/problem?take=25`

The problem endpoint first loads customers and then performs additional queries for each customer to compute aggregates. This means:
- one initial query for the customer list
- additional queries per customer for counts and totals
- database round-trips increase with the number of customers

## Mitigation

**Endpoint**: `GET /05-n-plus-1-data-access/mitigation?take=25`

The mitigation endpoint computes the required customer data in a single set-based query. This means:
- customer data and aggregates are fetched together
- the number of database round-trips stays low
- the operation scales more predictably

## Simulation notes

This scenario uses a small relational dataset, but it represents a very common production anti-pattern: issuing queries inside a loop instead of pushing aggregation into the database engine.

## How to try it

```bash
curl "http://localhost:5000/05-n-plus-1-data-access/problem?take=25"
curl "http://localhost:5000/05-n-plus-1-data-access/mitigation?take=25"
```

Try increasing `take` to make the difference more visible.

## What to observe

- Both endpoints should return the same logical customer summaries.
- The problem endpoint performs repeated database calls.
- The mitigation endpoint executes a single set-based query.
- The performance gap grows as `take` increases.

## Tools to use

This scenario can usually be understood from returned data and query logging.

Optional if you want to measure runtime impact:
- `dotnet-counters`
- a load tool (`wrk` or `bombardier`)

See [tools README](tools/README.md).

## Diagnostic tools

Use these tools to observe the behavior:
- database query logging → confirm repeated queries vs single query
- `wrk` or another load generator → amplify the difference under concurrent load
- `dotnet-counters` → watch allocation rate and runtime churn under repeated requests

Example:

```bash
wrk -t4 -c20 -d30s "http://localhost:5000/05-n-plus-1-data-access/problem?take=100"
wrk -t4 -c20 -d30s "http://localhost:5000/05-n-plus-1-data-access/mitigation?take=100"
```

## Source files

- Endpoint: [../src/ProductionDiagnosticsLab.Api/Endpoints/NPlusOneQueryEndpoints.cs](../src/ProductionDiagnosticsLab.Api/Endpoints/NPlusOneQueryEndpoints.cs)
- Tests: [../tests/ProductionDiagnosticsLab.Tests/Scenarios/DataAccessScenarioTests.cs](../tests/ProductionDiagnosticsLab.Tests/Scenarios/DataAccessScenarioTests.cs)

## Related scenarios

- [Scenario 01: Slow Data Access](01-slow-data-access.md)
- [Scenario 08: Large Response Buffering vs Streaming](08-large-response.md)

## External references

- [Efficient Querying - EF Core](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- [Performance Diagnosis - EF Core](https://learn.microsoft.com/en-us/ef/core/performance/performance-diagnosis)
- [Single vs. Split Queries - EF Core](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)
