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

> Documentation links to Microsoft Learn point to the latest docs; the lab itself targets .NET 8.

Each scenario has these paths:

- **Problem** — demonstrates the anti-pattern or failure mode.
- **Mitigation** — demonstrates a hand-written safer implementation.

Where .NET ships a ready-made solution (for example `HybridCache` for cache stampede or `AddStandardResilienceHandler()` for retries), the mitigation endpoint's code comments and the scenario doc point to it.

## Repository layout

```text
.
├── docs/
│   ├── data-access/                   # scenarios 01-02
│   ├── request-execution/             # scenarios 03-05
│   ├── observability/                 # scenarios 06-08
│   ├── dependency-reliability/        # scenarios 09-13
│   ├── memory/                        # scenarios 14-16
│   ├── startup/                       # scenarios 17-19
│   └── tools/README.md
├── src/
│   └── DiagnosticsLab.Api/
│       ├── Scenarios/
│       │   ├── DataAccess/
│       │   ├── DependencyReliability/
│       │   ├── Memory/
│       │   ├── Observability/
│       │   ├── RequestExecution/
│       │   ├── Startup/
│       │   └── ScenarioRegistration.cs   # DI registration and route mapping for every scenario
│       ├── Configuration/ Data/ Json/ Services/
│       └── Program.cs
├── tests/
│   └── DiagnosticsLab.Api.Tests/
│       ├── Scenarios/                     # one folder per theme, mirroring src
│       ├── Smoke/
│       └── Infrastructure/
├── dotnet-production-diagnostics-lab.slnx
└── README.md
```

Scenario endpoint classes live under `src/DiagnosticsLab.Api/Scenarios/<Theme>/`, and tests mirror that structure. The solution targets .NET 8 (`net8.0`) and uses the `.slnx` solution format, which requires a recent .NET SDK (9.0.200 or later).

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

- [Scenario 01: Excessive Data Materialization](docs/data-access/01-excessive-data-materialization.md)
- [Scenario 02: N + 1 Queries](docs/data-access/02-n-plus-one-queries.md)

### Request execution and runtime pressure

- [Scenario 03: Missing Cancellation Propagation](docs/request-execution/03-missing-cancellation-propagation.md)
- [Scenario 04: Blocking Request Thread](docs/request-execution/04-blocking-request-thread.md)
- [Scenario 05: ThreadPool Starvation](docs/request-execution/05-threadpool-starvation.md)

### Observability and operational visibility

- [Scenario 06: Missing Log Context](docs/observability/06-missing-log-context.md)
- [Scenario 07: Missing Health Probes](docs/observability/07-missing-health-probes.md)
- [Scenario 08: Logging and Audit Failure Propagation](docs/observability/08-logging-and-audit-failure-propagation.md)

### Dependency reliability

- [Scenario 09: Missing Dependency Timeouts](docs/dependency-reliability/09-missing-dependency-timeouts.md)
- [Scenario 10: Unbounded Retries](docs/dependency-reliability/10-unbounded-retries.md)
- [Scenario 11: Socket Exhaustion](docs/dependency-reliability/11-socket-exhaustion.md)
- [Scenario 12: Cache Stampede](docs/dependency-reliability/12-cache-stampede.md)
- [Scenario 13: Repeated Dependency Failures](docs/dependency-reliability/13-repeated-dependency-failures.md)

### Memory and payload handling

- [Scenario 14: Large Response Buffering](docs/memory/14-large-response-buffering.md)
- [Scenario 15: Request Body Memory Pressure](docs/memory/15-request-body-memory-pressure.md)
- [Scenario 16: LOH Fragmentation](docs/memory/16-loh-fragmentation.md)

### Startup, configuration, and deployment behavior

- [Scenario 17: Invalid Configuration](docs/startup/17-invalid-configuration.md)
- [Scenario 18: Silent Startup Failure](docs/startup/18-silent-startup-failure.md)
- [Scenario 19: Native AOT Serialization Failure](docs/startup/19-native-aot-serialization-failure.md)

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

The tests (in `tests/DiagnosticsLab.Api.Tests`) include:

- smoke checks (every GET route and health probes)
- scenario behavior checks, grouped by theme
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
