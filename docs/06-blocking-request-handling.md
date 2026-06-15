## Scenario 6: Blocking request handling

### Problem

GET /06-blocking-request-handling/problem?delayMs=500

Blocks the request thread using Thread.Sleep.

Blocking request threads reduces throughput and can lead to thread pool starvation under load.

---

### Improved version

GET /06-blocking-request-handling/improved?delayMs=500

Performs asynchronous waiting using Task.Delay and supports CancellationToken.

The request thread is released while waiting, allowing better scalability.

---

### Key issue

Blocking the ThreadPool using synchronous operations prevents efficient request processing.

---

### What to observe

- Problem endpoint blocks the request thread
- Improved endpoint yields during waiting
- Under load, blocking reduces system throughput
- Both endpoints return the same logical result but differ in execution model
