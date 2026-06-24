# .NET Production Diagnostics Lab

A hands-on ASP.NET Core lab for practicing production diagnostics with realistic failure modes.

This repository is intentionally small and focused. It is not a production template, architecture reference, or framework showcase. The goal is to make common production problems easy to trigger, observe, explain, and fix.

## What you will learn

This lab demonstrates practical .NET and ASP.NET Core diagnostics scenarios across reliability, performance, memory, data access, observability, and deployment behavior.

You will work with examples of:

- inefficient data access that materializes too much data or performs too many database round-trips
- missing cancellation, missing timeouts, and repeated dependency failures
- blocking request execution and ThreadPool pressure
- weak logging context and missing health probes
- unsafe side effects, such as logging or audit failures breaking business operations
- request and response buffering that creates memory pressure
- socket exhaustion caused by poor HTTP client lifetime management
- Large Object Heap pressure and fragmentation
- Native AOT serialization failures caused by reflection-based JSON metadata

Each scenario has two paths:

- **Problem** — demonstrates the anti-pattern or failure mode.
- **Mitigation** — demonstrates the safer implementation or operationally better behavior.

## Repository layout

```text
.
├── docs/
│   ├── 01-excessive-data-materialization.md
│   ├── ...
│   ├── 19-repeated-dependency-failures.md
│   └── tools/README.md
├── src/
│   └── DiagnosticsLab.Api/
├── tests/
│   └── DiagnosticsLab.Tests/
└── README.md
```

The exact project names may evolve, but the intent stays the same: one API project hosts the scenarios, and the test project validates endpoint behavior.

## Quick start

Restore dependencies:

```bash
dotnet restore
```

Run the API:

```bash
dotnet run --project src/DiagnosticsLab.Api
```

Run tests:

```bash
dotnet test
```

The API uses SQLite and creates local seed data automatically.

## How to use the lab

1. Open one scenario document from the list below.
2. Run the **problem** endpoint and observe the behavior.
3. Run the **mitigation** endpoint and compare the result.
4. Use logs, response payloads, tests, or diagnostic tools to confirm the difference.
5. Read the source file linked from the scenario document to see the implementation.

Most scenarios can be understood from endpoint behavior and logs. Some scenarios become clearer under repeated requests or load.

For optional diagnostic tooling, see [docs/tools/README.md](docs/tools/README.md).

## Scenarios

### Data access

- [Scenario 01: Excessive Data Materialization](docs/01-excessive-data-materialization.md)
- [Scenario 05: N + 1 Queries](docs/05-n-plus-one-queries.md)

### Request execution and runtime pressure

- [Scenario 02: Missing Cancellation Propagation](docs/02-missing-cancellation-propagation.md)
- [Scenario 06: Blocking Request Thread](docs/06-blocking-request-thread.md)
- [Scenario 15: ThreadPool Starvation](docs/15-threadpool-starvation.md)

### Observability and operational visibility

- [Scenario 03: Missing Log Context](docs/03-missing-log-context.md)
- [Scenario 10: Missing Health Probes](docs/10-missing-health-probes.md)
- [Scenario 12: Logging and Audit Failure Propagation](docs/12-logging-and-audit-failure-propagation.md)

### Dependency reliability

- [Scenario 04: Missing Dependency Timeouts](docs/04-missing-dependency-timeouts.md)
- [Scenario 07: Unbounded Retries](docs/07-unbounded-retries.md)
- [Scenario 14: Socket Exhaustion](docs/14-socket-exhaustion.md)
- [Scenario 17: Cache Stampede](docs/17-cache-stampede.md)
- [Scenario 19: Repeated Dependency Failures](docs/19-repeated-dependency-failures.md)

### Memory and payload handling

- [Scenario 08: Large Response Buffering](docs/08-large-response-buffering.md)
- [Scenario 13: Request Body Memory Pressure](docs/13-request-body-memory-pressure.md)
- [Scenario 16: LOH Fragmentation](docs/16-loh-fragmentation.md)

### Startup, configuration, and deployment behavior

- [Scenario 09: Invalid Configuration](docs/09-invalid-configuration.md)
- [Scenario 11: Silent Startup Failure](docs/11-silent-startup-failure.md)
- [Scenario 18: Native AOT Serialization Failure](docs/18-native-aot-serialization-failure.md)

## Diagnostic tools

External tools are optional. Use them when you want to observe runtime behavior under load or inspect a process more deeply.

The tools guide covers:

- `dotnet-counters`
- `dotnet-trace`
- `dotnet-dump`
- `dotnet-gcdump`
- `dotnet-stack`
- `wrk`
- `bombardier`
- `netstat`
- `ss`

See [docs/tools/README.md](docs/tools/README.md) for installation and minimal commands.

## Tests

Run all tests:

```bash
dotnet test
```

The tests include:

- smoke checks
- scenario behavior checks
- startup validation checks
- endpoint response checks

Use tests as a safety net when renaming routes, changing scenario behavior, or consolidating documentation.

## Documentation conventions

Scenario names are problem-first. The title should describe the failure mode, not the mitigation.

Good examples:

- **Missing Dependency Timeouts**
- **Socket Exhaustion**
- **Cache Stampede**
- **Repeated Dependency Failures**

Each scenario document should include:

- goal
- why the problem matters
- problem endpoint behavior
- mitigation endpoint behavior
- how to try it
- what to observe
- source files
- related scenarios
- external references where useful

## Notes

The examples are intentionally compact. They are designed for learning and diagnostics practice, not for direct copy-paste into production systems.

The important part is the comparison between the problem path and the mitigation path. That comparison makes the production failure mode visible and testable.
