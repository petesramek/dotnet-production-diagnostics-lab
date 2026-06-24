## Scenario 19: Repeated Dependency Failures

### Goal
  
Show why a service should stop calling a dependency after repeated failures and how circuit-breaker behavior provides fail-fast protection.

### Why this matters
  
When a dependency keeps failing, continuing to call it on every request wastes resources and adds pressure to an already unhealthy path. After enough failures, the service should fail fast for a period instead of sending more calls that are unlikely to succeed.

### Problem
  
**Endpoint**: `GET /19-repeated-dependency-failures/problem?mode=FAIL`
The problem endpoint calls the dependency on every request without circuit-breaker protection. This means:
- every failing request still reaches the dependency
- repeated failures continue to consume request and dependency capacity
- the service has no fail-fast behavior when the dependency is clearly unhealthy

### Mitigation
  
**Endpoint**: `GET /19-repeated-dependency-failures/mitigation?mode=FAIL`
The mitigation endpoint uses circuit-breaker behavior on the dependency client. This means:
- initial failures are still observed normally
- after the failure threshold is crossed, later requests stop calling the dependency temporarily
- callers receive a fail-fast response while the circuit is open
- after the break duration expires, the dependency can be probed again for recovery

### Simulation notes
  
This scenario uses an HTTP dependency client with a simulated failing downstream service. The mitigation path focuses only on circuit-breaker behavior. Timeout and retry are covered separately in earlier scenarios.

### How to try it
  
Call the problem endpoint repeatedly with a failing dependency mode:

```bash
curl "http://localhost:5000/19-repeated-dependency-failures/problem?mode=FAIL"
curl "http://localhost:5000/19-repeated-dependency-failures/problem?mode=FAIL"
curl "http://localhost:5000/19-repeated-dependency-failures/problem?mode=FAIL"
```

Then call the mitigation endpoint repeatedly with the same failing mode:

```bash
curl "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=FAIL"
curl "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=FAIL"
curl "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=FAIL"
curl "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=FAIL"
```

To verify recovery after the break duration expires, wait for the breaker window to pass and then call:

```bash
curl "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=OK"
```

### What to observe
- In the problem path, repeated failing requests keep calling the dependency.
- In the mitigation path, the first failing calls still reach the dependency.
- After the breaker opens, later mitigation requests should report that the dependency was not called.
- After the break duration expires, a successful request should close the circuit again.

### Tools to use
  
Use external tooling when you want to make repeated failing calls and fail-fast behavior visible.  
Suggested tools:
- logs
- curl
- optional a load tool (`wrk` or `bombardier`)  
See [tools README](tools/README.md).

### Diagnostic tools
  
Use these tools to observe the difference:
- application logs → confirm dependency invocations and fail-fast behavior
- curl → observe repeated failures and blocked calls directly
- wrk or another load generator → amplify the difference between repeated outbound calls and fail-fast behavior under concurrency  
Example:

```bash
wrk -t4 -c20 -d15s "http://localhost:5000/19-repeated-dependency-failures/problem?mode=FAIL"
wrk -t4 -c20 -d15s "http://localhost:5000/19-repeated-dependency-failures/mitigation?mode=FAIL"
```

### Source files
- Endpoint: [../src/DiagnosticsLab.Api/Endpoints/RepeatedDependencyFailuresEndpoints.cs](../src/DiagnosticsLab.Api/Endpoints/RepeatedDependencyFailuresEndpoints.cs)
- Tests: [../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs](../tests/DiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs)

### Related scenarios
- [Scenario 04: Missing Dependency Timeouts](04-missing-dependency-timeouts.md)
- [Scenario 07: Unbounded Retries](07-unbounded-retries.md)

### External references
- [Build resilient HTTP apps: Key development patterns - .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Introduction to resilient app development - .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- [Circuit Breaker pattern - Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
