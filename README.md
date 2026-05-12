# player-api-tests

REST API + NUnit test suite (unit + integration) for player management.
Built as a QA Engineering Assessment submission.

[![CI](https://github.com/subbotin-es/player-api-tests/actions/workflows/dotnet.yml/badge.svg)](https://github.com/subbotin-es/player-api-tests/actions/workflows/dotnet.yml)

**Author:** Evgenii Subbotin — evgenii@subbotin.es
**Portfolio:** subbotin.es | **GitHub:** github.com/subbotin-es | **LinkedIn:** linkedin.com/in/evgenii-subbotin/

## Live API

| | URL |
|---|---|
| Base URL | https://player-api-tests-production.up.railway.app |
| Swagger UI | https://player-api-tests-production.up.railway.app/swagger |

> **Note:** Hosted on Railway free tier. First request after inactivity may take ~5 seconds to wake.

## Quick Start (local)

```bash
dotnet restore
dotnet build
dotnet test
```

## Run API locally

```bash
dotnet run --project PlayerApi
# Swagger UI: http://localhost:5000/swagger
```

## Architecture

- **PlayerApi** — ASP.NET Core 8 Web API, in-memory store, JWT auth, Swagger
- **PlayerApi.Tests** — NUnit 3, two test layers:
  - Unit: PlayerStore tested directly (new PlayerStore(), no HTTP)
  - Integration: WebApplicationFactory (in-process, no real port)

## Test Strategy

All state lives in a singleton `PlayerStore` (ConcurrentDictionary).
Unit tests instantiate PlayerStore directly — fast, isolated, no infrastructure.
Integration tests use WebApplicationFactory<Program> — no server process, no ports.
Store is cleared in TestBase.SetUp to guarantee isolation between fixture classes.
All test data is declared in Fixtures/PlayerFixtures.cs — never hardcoded inline.

## CI / Test Reports

GitHub Actions runs on every push to main and develop.
Green badge = all unit + integration tests pass.

Latest CI runs and test results via NUnit — https://github.com/subbotin-es/player-api-tests/actions/workflows/dotnet.yml

## Performance Regression Pack

A companion JMeter performance test suite runs against the live Railway API to gate for regressions and document system limits under realistic concurrency.

**Repository:** [player-api-tests-performance](https://github.com/subbotin-es/player-api-tests-performance)
**Results dashboard (GitHub Pages):** https://subbotin-es.github.io/player-api-tests-performance/

### Approach

Three JMeter 5.6.3 test plans cover the full CRUD lifecycle with JWT authentication:

| Plan | Users | Duration | Purpose |
|---|---|---|---|
| Smoke | 5 | 30 s | Regression gate — catches breakage before baseline runs |
| Baseline | 10 | 60 s | Trend tracking — establishes p95/p99 reference point |
| Stress | 5 → 15 → 30 (stepped) | ~5 min | Degradation identification under rising concurrency |

JWT tokens are extracted via JMeter's regex engine and injected at thread-group level after login succeeds. All execution is CLI-only; HTML dashboards are generated per run and published to GitHub Pages on every push to `main`. Full dashboards are also retained as CI artifacts for 30 days.

### Key Results

| Stage | Users | Avg latency | p99 | Error rate |
|---|---|---|---|---|
| Smoke | 5 | < 200 ms | — | 0% |
| Baseline | 10 | ~171 ms | — | < 0.5% |
| Stress (at limit) | ~25 | degraded | ~7.2 s | 3–5% (502/503) |

JWT login accounts for 15–20% of each iteration's wall-clock time across all concurrency levels. Login latency scales roughly linearly with concurrency (CPU-bound signing), while CRUD operations degrade faster due to dictionary write contention.

### Limitations

- **Infrastructure ceiling:** Railway free tier hard-limits containers to 512 MB RAM. At approximately 25 concurrent users, memory saturation triggers container restart cycles and transient gateway errors (502/503). This is an infrastructure constraint, not an application defect.
- **In-memory store contention:** `ConcurrentDictionary` uses fine-grained striped locking. The p99/p95 ratio climbs from 1.3× at 5 users to 2.6× at 30 users on write-heavy operations (POST create, DELETE). A production database would exhibit different — not necessarily better — contention characteristics.
- **Cold-start artifact:** A dormant Railway instance adds 4–6 s to the first request. Mitigated by a dedicated warm-up thread group and a CI pre-flight request; warm-up samples are excluded from all reported metrics.

### Assumptions

- Warm-up phase fully isolates cold-start latency; reported averages reflect steady-state behaviour only.
- JWT signing overhead is treated as inherent baseline cost and is not optimised away in test results.
- Error rates above 1% at the stress stage are attributed to the 512 MB RAM ceiling rather than application logic errors.

## Documentation

- docs/ADR-001 — Why in-memory storage
- docs/ADR-002 — Why WebApplicationFactory
- docs/ADR-003 — Why stateless JWT
- docs/SECURITY.md — OWASP relevance
- docs/TEST-PLAN.md — Coverage goals, data model, coverage matrix

## AI Engineering

Built with Claude Code as primary engineering accelerator.
All architectural decisions are human-authored and documented in /docs.