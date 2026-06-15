## Scenario 9: Invalid configuration

Uses configuration without validating it.### Problem

If configuration is invalid or missing:
- the application still starts
- failures occur at runtime
- errors appear only when the endpoint is executed

---

### Improved version

GET /09-invalid-configuration/improved

Uses configuration validated at startup via IOptions<T> and ValidateOnStart().

Invalid configuration is detected early and prevents the application from running in a broken state.

---

### Key issue

The system allows invalid configuration to be used at runtime instead of validating it upfront.

---

### What to observe

- Remove or break configuration values
- Problem endpoint fails at runtime
- Improved setup fails during application startup

GET /09-invalid-configuration/problem

