# Scenario 3: Poor observability

## Problem

`POST /api/payments/problem` logs vague messages without enough context.

This makes production troubleshooting harder because logs do not explain which payment, customer, or input caused the issue.

## Improved version

`POST /api/payments/observable` uses structured logging and a logging scope with payment and customer identifiers.

## What to observe

- Compare log messages from both endpoints.
- Check whether logs contain enough context to diagnose failures.
- Notice that useful logs are specific without exposing sensitive information.
