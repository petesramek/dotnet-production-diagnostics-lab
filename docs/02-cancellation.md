# Scenario 2: Missing cancellation

## Problem

`GET /api/reports/slow` simulates a long-running operation without cancellation support.

When clients disconnect or requests time out, work may continue unnecessarily.

## Improved version

`GET /api/reports/cancellable` accepts and passes a `CancellationToken` to the long-running operation.

## What to observe

- Cancel the request from a client.
- Compare whether the operation can stop early.
- Check logs around request start and completion.
