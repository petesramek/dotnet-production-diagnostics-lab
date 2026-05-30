# Scenario 12: Logging failure isolation

## Problem

`POST /api/audit/problem` directly depends on a simulated audit sink. If the sink fails, the business operation fails too.

That may be correct for strict audit requirements, but it is harmful when the audit/logging sink is non-critical and should not break the user-facing operation.

## Improved version

`POST /api/audit/improved` isolates non-critical audit sink failure from the business operation.

The failure is still logged, but the endpoint returns success for the business operation.

## What to observe

- Send `{ "operationId": "00000000-0000-0000-0000-000000000001", "auditShouldFail": true }` to both endpoints.
- The problem endpoint fails when audit fails.
- The improved endpoint succeeds while logging the audit failure.
- This scenario is about failure isolation, not ignoring important audit requirements.
