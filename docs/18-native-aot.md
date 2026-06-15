## Scenario 18: Native AOT serialization

### Problem

POST /18-native-aot/problem

Uses reflection-based JSON serialization.

Under Native AOT:
- reflection metadata may be trimmed
- deserialization can fail or behave incorrectly

---

### Improved version

POST /18-native-aot/improved

Uses System.Text.Json source generators.

This:
- generates serialization metadata at compile time
- is fully compatible with Native AOT
- avoids runtime reflection

---

### Key issue

The problem is relying on reflection-based serialization in environments where metadata trimming is applied.

---

### What to observe

Build with Native AOT:

dotnet publish -c Release -p:PublishAot=true

Run application and test both endpoints.

Expected:
- problem endpoint may fail or behave unexpectedly
- improved endpoint works reliably

---

### Diagnostic signals

- runtime exceptions in problem endpoint
- missing metadata errors
- correct behavior in improved version
