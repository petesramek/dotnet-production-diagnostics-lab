# Scenario 6: Blocking request handling

## Problem

`GET /api/blocking/problem` blocks the request thread with `Thread.Sleep`.

Blocking request threads can reduce throughput under concurrent load and may contribute to thread pool starvation symptoms in busy ASP.NET Core applications.

## Improved version

`GET /api/blocking/improved` uses asynchronous waiting and accepts a `CancellationToken`.

## What to observe

- The problem endpoint blocks the request thread for the requested delay.
- The improved endpoint yields while waiting.
- Under concurrency, blocking code wastes request-processing capacity.
