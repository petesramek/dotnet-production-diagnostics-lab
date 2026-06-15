## Scenario 14: Socket exhaustion

### Problem

GET /14-socket-exhaustion/problem

Creates a new HttpClient instance for each request.

This causes:
- new TCP connections for each call
- accumulation of sockets in TIME_WAIT state
- eventual exhaustion of available ports

Under load, this leads to:
- SocketException
- failed outbound calls
- degraded system reliability

---

### Improved version

GET /14-socket-exhaustion/improved

Uses IHttpClientFactory to manage HttpClient instances.

This enables:
- connection reuse
- handler pooling
- controlled socket lifetime

---

### Key issue

The problem is creating HttpClient instances per request instead of reusing them.

---

### What to observe

Run a load test:

wrk -t4 -c50 http://localhost:5000/14-socket-exhaustion/problem

Then inspect sockets:

netstat -an | findstr TIME_WAIT

You will observe a large number of connections.

Repeat for improved endpoint:

wrk -t4 -c50 http://localhost:5000/14-socket-exhaustion/improved

Connection count remains stable due to reuse.

---

### Diagnostic tools

- netstat → connection states
- dotnet-counters → System.Net.Http metrics
- wrk → load generation
