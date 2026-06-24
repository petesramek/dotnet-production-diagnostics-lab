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
- Configuration problems detected too late instead of at startup
- Missing health checks for operational visibility
- Startup work that fails silently instead of failing fast
- Logging or audit sink failures leaking into business operations
- Request body memory pressure caused by buffering large uploads

## How to run

```bash
dotnet restore
dotnet run --project src/DiagnosticsLab.Api
```

The API uses SQLite and creates local seed data automatically.

## Scenarios

### 1. [Slow data access](docs/01-slow-data-access.md)

- Problem: `GET /api/orders/slow?customerId=42`
- Improved: `GET /api/orders/improved?customerId=42`

### 2. [Cancellation](docs/02-missing-cancellation-propagation.md)

- Problem: `GET /api/reports/slow`
- Improved: `GET /api/reports/cancellable`

### 3. [Observability](docs/03-missing-log-context.md)

- Problem: `POST /api/payments/problem`
- Improved: `POST /api/payments/observable`

### 4. [Missing Dependency Timeouts](docs/04-missing-dependency-timeouts.md)

- Problem: `GET /api/shipping/problem?country=CZ`
- Improved: `GET /api/shipping/resilient?country=CZ`

### 5. [N + 1 Qery Problem](docs/05-chatty-data-access.md)

- Problem: `GET /api/customers/problem?take=25`
- Improved: `GET /api/customers/improved?take=25`

### 6. [Blocking request handling](docs/06-blocking-request-handling.md)

- Problem: `GET /api/blocking/problem?delayMs=500`
- Improved: `GET /api/blocking/improved?delayMs=500`

### 7. [Retry storm / controlled retry](docs/07-retry-storm.md)

- Problem: `GET /api/inventory/problem?sku=FAIL`
- Improved: `GET /api/inventory/improved?sku=FAIL`

### 8. [Large Response Buffering](docs/08-large-response-streaming.md)

- Problem: `GET /api/exports/problem?rows=1000`
- Improved: `GET /api/exports/improved?rows=1000`

### 9. [Configuration validation](docs/09-configuration-validation.md)

- Problem: `GET /api/config/problem`
- Improved: application startup validates `ExternalServices:BillingApiBaseUrl`

### 10. [Health checks](docs/10-health-checks.md)

- Improved: `GET /health/live` and `GET /health/ready`

### 11. [Startup exception handling](docs/11-startup-exception-handling.md)

- Problem: `GET /api/startup/problem`
- Improved: `GET /api/startup/improved`

### 12. [Logging failure isolation](docs/12-logging-and-audit-failure-propagation-isolation.md)

- Problem: `POST /api/audit/problem`
- Improved: `POST /api/audit/improved`

### 13. [Request body memory pressure](docs/13-request-body-memory-pressure.md)

- Problem: `POST /api/uploads/problem`
- Improved: `POST /api/uploads/improved`

## Tests

```bash
dotnet test
```

The tests include smoke checks, scenario checks, startup validation checks, and endpoint behavior checks.

## Notes

The examples are intentionally small. The goal is to make production-style problems easy to see, explain, and improve.
