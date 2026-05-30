# .NET Production Diagnostics Lab

A small ASP.NET Core diagnostics lab showing common production-style problems, how to detect them, and how to improve reliability, performance, and observability.

This repository is a hands-on lab, not a production template and not an architecture showcase.

## What this repo demonstrates

- Slow data access caused by inefficient queries
- N+1 / chatty data access caused by per-row database calls
- Missing cancellation and timeout behavior
- Sync-over-async / blocking request handling
- Poor observability and weak logging
- Reliability issues caused by unsafe external dependency calls
- Retry storms caused by uncontrolled retries
- Memory pressure caused by building large responses in memory

## How to run

```bash
dotnet restore
dotnet run --project src/DiagnosticsLab.Api
```

The API uses SQLite and creates local seed data automatically.

## Scenarios

### 1. Slow data access

- Problem: `GET /api/orders/slow?customerId=42`
- Improved: `GET /api/orders/improved?customerId=42`
- Notes: `docs/01-slow-data-access.md`

### 2. Cancellation

- Problem: `GET /api/reports/slow`
- Improved: `GET /api/reports/cancellable`
- Notes: `docs/02-cancellation.md`

### 3. Observability

- Problem: `POST /api/payments/problem`
- Improved: `POST /api/payments/observable`
- Notes: `docs/03-observability.md`

### 4. External dependency reliability

- Problem: `GET /api/shipping/problem?country=CZ`
- Improved: `GET /api/shipping/resilient?country=CZ`
- Notes: `docs/04-external-dependencies.md`

### 5. N+1 / chatty data access

- Problem: `GET /api/customers/problem?take=25`
- Improved: `GET /api/customers/improved?take=25`
- Notes: `docs/05-chatty-data-access.md`

### 6. Blocking request handling

- Problem: `GET /api/blocking/problem?delayMs=500`
- Improved: `GET /api/blocking/improved?delayMs=500`
- Notes: `docs/06-blocking-request-handling.md`

### 7. Retry storm / controlled retry

- Problem: `GET /api/inventory/problem?sku=FAIL`
- Improved: `GET /api/inventory/improved?sku=FAIL`
- Notes: `docs/07-retry-storm.md`

### 8. Large response allocation / streaming

- Problem: `GET /api/exports/problem?rows=1000`
- Improved: `GET /api/exports/improved?rows=1000`
- Notes: `docs/08-large-response-streaming.md`

## Tests

```bash
dotnet test
```

The tests include smoke checks and scenario checks proving that selected problem/improved endpoints return equivalent logical data while the improved versions use safer implementation patterns.

## Notes

The examples are intentionally small. The goal is to make production-style problems easy to see, explain, and improve.
