# Scenario 7: Retry storm / controlled retry

## Problem

`GET /api/inventory/problem` retries immediately when the simulated inventory provider fails.

Immediate retries can make an outage worse because every request adds more pressure to a dependency that is already failing.

## Improved version

`GET /api/inventory/improved` uses fewer controlled attempts, timeout awareness, small backoff delays, and structured logs.

## What to observe

- Call `/api/inventory/problem?sku=FAIL`.
- Call `/api/inventory/improved?sku=FAIL`.
- Compare the number of attempts and log messages.
- Notice that the improved endpoint fails in a controlled way instead of retrying aggressively.
