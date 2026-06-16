# Scenario 18: Native AOT Serialization

## Goal

Show why reflection-based JSON serialization is fragile under Native AOT and why source-generated metadata is the safer approach.

## Why this matters

Native AOT trims unused metadata aggressively. Code that relies on runtime reflection for serialization may work during normal JIT execution but fail, throw, or behave unpredictably when published as a Native AOT application.

## Problem

**Endpoint**: `POST /18-native-aot/problem`

The problem endpoint uses reflection-based JSON serialization. This means:
- required metadata may be trimmed at publish time
- deserialization can fail under Native AOT
- runtime behavior depends on metadata that may no longer exist

## Mitigation

**Endpoint**: `POST /18-native-aot/mitigation`

The mitigation endpoint uses `System.Text.Json` source generation. This means:
- serialization metadata is generated at compile time
- runtime reflection is not required for the payload type
- the endpoint remains compatible with Native AOT publishing

## Simulation notes

In regular development builds both endpoints may appear to work. The real difference becomes visible when the app is published with Native AOT enabled and trimming is applied.

## How to try it

Run the endpoints normally first:

```bash
curl -X POST "http://localhost:5000/18-native-aot/problem"   -H "Content-Type: application/json"   -d '{"name":"Pete","age":30}'

curl -X POST "http://localhost:5000/18-native-aot/mitigation"   -H "Content-Type: application/json"   -d '{"name":"Pete","age":30}'
```

Then publish with Native AOT:

```bash
dotnet publish -c Release -p:PublishAot=true
```

Run the published application and repeat the requests.

## What to observe

- In a normal development build, both endpoints may succeed.
- Under Native AOT, the problem endpoint may fail or behave incorrectly.
- The mitigation endpoint should continue to work because it uses source-generated metadata.
- The response payload distinguishes the paths with `sourceGenerated = false` vs `true`.

## Diagnostic tools

Use these tools to validate the behavior:
- Native AOT publish output → confirm publish mode and potential warnings
- application logs / runtime exceptions → capture missing metadata failures
- integration or smoke tests against the published binary → verify behavior outside the normal development host

## Source files

- Endpoint: [../src/ProductionDiagnosticsLab.Api/Endpoints/NativeAotEndpoints.cs](../src/ProductionDiagnosticsLab.Api/Endpoints/NativeAotEndpoints.cs)
- JSON context: [../src/ProductionDiagnosticsLab.Api/NativeAotJsonContext.cs](../src/ProductionDiagnosticsLab.Api/NativeAotJsonContext.cs)
- Tests: [../tests/ProductionDiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs](../tests/ProductionDiagnosticsLab.Tests/Scenarios/ReliabilityScenarioTests.cs)
- Smoke tests: [../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs](../tests/ProductionDiagnosticsLab.Tests/Smoke/ApiSmokeTests.cs)

## Related scenarios

- [Scenario 09: Configuration Validation](09-invalid-configuration.md)
- [Scenario 16: LOH Fragmentation](16-loh-fragmentation.md)

## External references

- [Native AOT deployment - .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [ASP.NET Core support for Native AOT](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot?view=aspnetcore-10.0)
- [How to use source generation in System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
