# Scenario 13: Request body memory pressure

## Problem

`POST /api/uploads/problem` reads the entire request body into memory.

That is simple, but it can create memory pressure when uploads are large or many uploads happen concurrently.

## Improved version

`POST /api/uploads/improved` processes the request body incrementally and enforces a maximum size.

The endpoint calculates a SHA-256 hash while reading chunks, without buffering the entire body in memory.

## What to observe

- Send a small request body to both endpoints.
- The problem endpoint reports `streamed: false`.
- The improved endpoint reports `streamed: true` and returns a SHA-256 hash.
- Try a request body larger than the configured limit and observe the controlled rejection.
