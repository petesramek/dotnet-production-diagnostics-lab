# Scenario 8: Large response allocation / streaming

## Problem

`GET /api/exports/problem` builds the full export result in memory before returning the response.

This pattern is simple, but it can create memory pressure when exports grow large or multiple users request exports at the same time.

## Improved version

`GET /api/exports/improved` returns an `IAsyncEnumerable<T>` so rows can be produced incrementally.

## What to observe

- Call both endpoints with a small row count first, for example `rows=100`.
- Increase the row count and observe the implementation difference.
- The improved endpoint avoids creating one large list before returning data.
