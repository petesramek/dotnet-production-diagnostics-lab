# Scenario 1: Slow data access

## Problem

`GET /api/orders/slow` loads all orders into memory and only then filters by customer.

This is a common production issue: the endpoint may work with small data sets, but becomes slower and more memory-heavy as data grows.

## Improved version

`GET /api/orders/improved` filters in the database, uses `AsNoTracking`, projects only the required fields, limits the result set, and accepts a cancellation token.

## What to observe

- Compare logs between both endpoints.
- Compare behavior as the seed data grows.
- Look at where filtering happens: in memory vs. in the database query.
