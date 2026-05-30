# Scenario 9: Configuration validation

## Problem

`GET /api/config/problem` reads configuration lazily during request handling.

This means a deployment can start successfully even when required configuration is missing or invalid. The failure appears only when a user or background job reaches the affected path.

## Improved version

The application validates `ExternalServices:BillingApiBaseUrl` during startup using options validation and `ValidateOnStart()`.

## What to observe

- Remove or invalidate `ExternalServices:BillingApiBaseUrl` in configuration.
- The improved setup fails fast during startup.
- The problem endpoint demonstrates the weaker pattern where configuration is discovered too late.
