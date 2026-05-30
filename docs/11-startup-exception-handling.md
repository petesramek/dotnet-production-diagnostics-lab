# Scenario 11: Startup exception handling

## Problem

`GET /api/startup/problem` simulates startup-style initialization that fails, but the failure is swallowed.

In real systems, swallowing startup failures can leave the application running in a partially initialized state. That makes deployments look successful even though the application is already broken.

## Improved version

`GET /api/startup/improved` reports the failure clearly and returns a controlled failure response.

In real startup code, the same principle usually means logging the error clearly and failing application startup instead of hiding the failure.

## What to observe

- The problem endpoint returns success even though initialization failed.
- The improved endpoint makes the failure visible.
- The improved endpoint logs the failure as an error instead of hiding it as a warning.
