# Scenario 4: External dependency reliability

## Problem

`GET /api/shipping/problem` calls a simulated external dependency without timeout control.

A slow dependency can tie up request handling and make the application appear unstable.

## Improved version

`GET /api/shipping/resilient` uses a timeout and returns a controlled gateway timeout response when the dependency is too slow.

## What to observe

- Call `/api/shipping/problem?country=SLOW`.
- Call `/api/shipping/resilient?country=SLOW`.
- Compare how long each request takes and how clearly failure is handled.
