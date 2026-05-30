# Scenario 5: N+1 / chatty data access

## Problem

`GET /api/customers/problem` loads customers and then queries order aggregates separately for each customer.

This is a common production issue. The endpoint can look fine during development, but the number of database roundtrips grows with the number of returned customers.

## Improved version

`GET /api/customers/improved` projects customers and order aggregates in a single query shape.

## What to observe

- The problem endpoint performs repeated database work per customer.
- The improved endpoint keeps the operation set-based.
- Both endpoints should return the same logical result for the same `take` value.
