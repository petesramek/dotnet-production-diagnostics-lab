# Tools README

> Intended repo path: `docs/tools/README.md`

This document exists for one purpose: **install the external tools used by selected scenarios and show the minimum commands needed to use them in this lab**.

## When you need this file
Most scenarios do **not** need external tooling.

Use this file mainly for:
- Scenario 04 — Missing Dependency Timeouts
- Scenario 06 — Blocking Request Handling
- Scenario 07 — Unbounded Retries
- Scenario 08 — Large Response Buffering
- Scenario 13 — Request Body Memory Pressure
- Scenario 14 — Socket Exhaustion
- Scenario 15 — ThreadPool starvation
- Scenario 16 — LOH Fragmentation
- Scenario 17 — Cache Stampede

Optional:
- Scenario 01 — Excessive Data Materialization
- Scenario 05 — 05 — N + 1 Queries

## 1. Install .NET diagnostics tools
Install these once on the machine where you run the lab:

```bash
dotnet tool install --global dotnet-counters
dotnet tool install --global dotnet-trace
dotnet tool install --global dotnet-dump
dotnet tool install --global dotnet-gcdump
dotnet tool install --global dotnet-stack
```

Update them if already installed:

```bash
dotnet tool update --global dotnet-counters
dotnet tool update --global dotnet-trace
dotnet tool update --global dotnet-dump
dotnet tool update --global dotnet-gcdump
dotnet tool update --global dotnet-stack
```

Verify installation:

```bash
dotnet-counters --version
dotnet-trace --version
dotnet-dump --version
dotnet-gcdump --version
dotnet-stack --version
```

## 2. Install load tools

### Option A — bombardier (best cross-platform default)
Download the binary from the project releases or install it by the method you use for your platform.

Repository:
- https://github.com/codesenberg/bombardier

Basic check:

```bash
bombardier --version
```

### Option B — wrk (common on Linux/macOS)
Clone and build from source:

```bash
git clone https://github.com/wg/wrk.git
cd wrk
make
```

Basic check:

```bash
./wrk --help
```

Repository:
- https://github.com/wg/wrk

## 3. Minimum commands you actually need

### dotnet-counters
Use this first when you suspect runtime pressure.

```bash
dotnet-counters ps
dotnet-counters monitor --process-id <pid> System.Runtime
```

Use for:
- memory pressure
- GC activity
- ThreadPool pressure
- general first-pass investigation

### dotnet-trace
Use this when counters say something is wrong but you need better evidence.

```bash
dotnet-trace ps
dotnet-trace collect --process-id <pid>
```

Use for:
- blocking analysis
- runtime timeline investigation
- starvation investigation

### dotnet-dump
Use this for hangs, crashes, or deeper memory analysis.

```bash
dotnet-dump ps
dotnet-dump collect --process-id <pid>
dotnet-dump analyze <dump-file>
```

### dotnet-gcdump
Use this for live managed heap snapshots.

```bash
dotnet-gcdump ps
dotnet-gcdump collect --process-id <pid>
```

### dotnet-stack
Use this when you want a quick look at managed thread stacks.

```bash
dotnet-stack ps
dotnet-stack report --process-id <pid>
```

### bombardier
Good default on Windows, Linux, and macOS.

```bash
bombardier http://localhost:5000/15-threadpool-starvation/problem?delayMs=500
bombardier -c 50 -d 30s http://localhost:5000/14-socket-exhaustion/problem
```

### wrk
Good when you want compact benchmarking commands or Lua scripts.

```bash
wrk -t4 -c50 -d30s http://localhost:5000/14-socket-exhaustion/problem
wrk -t4 -c20 -d30s "http://localhost:5000/07-unbounded-retries/problem?sku=FAIL"
```

## 4. OS-level commands for socket scenarios

### Windows — netstat
Use for Scenario 14.

```powershell
netstat -ano
netstat -no -p tcp
netstat -ano | findstr TIME_WAIT
```

### Linux — ss
Use for Scenario 14.

```bash
ss -tan
ss -tln
ss -tan state time-wait
ss -s
```

## 5. Which tool to use for which scenario

- **04** Missing Dependency Timeouts → load tool + `dotnet-counters`
- **06** Blocking Request Thread → load tool + `dotnet-counters`, optional `dotnet-trace`
- **07** Unbounded Retries → load tool + logs, optional `dotnet-counters`
- **08** Large Response Buffering → load tool + `dotnet-counters`
- **13** Request Body Memory Pressure → repeated requests + `dotnet-counters`
- **14** Socket Exhaustion → load tool + `netstat` or `ss` + optional `dotnet-counters`
- **15** ThreadPool Starvation → load tool + `dotnet-counters` + optional `dotnet-trace` / `dotnet-stack`
- **16** LOH Fragmentation → `dotnet-counters` + optional `dotnet-gcdump` + sustained load
- **17** Cache Stampede → load tool + logs

## 6. Official references
- .NET diagnostics tools overview: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/tools-overview
- dotnet-counters: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters
- dotnet-trace: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace
- dotnet-dump: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-dump
- dotnet-gcdump: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-gcdump
- dotnet-stack: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-stack
- Windows netstat: https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/netstat
- Windows port exhaustion guidance: https://learn.microsoft.com/en-us/troubleshoot/windows-client/networking/tcp-ip-port-exhaustion-troubleshooting
- Linux ss manual: https://www.man7.org/linux/man-pages/man8/ss.8.html
- bombardier: https://github.com/codesenberg/bombardier
- wrk: https://github.com/wg/wrk
