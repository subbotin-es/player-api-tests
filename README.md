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

## Documentation

- docs/ADR-001 — Why in-memory storage
- docs/ADR-002 — Why WebApplicationFactory
- docs/ADR-003 — Why stateless JWT
- docs/SECURITY.md — OWASP relevance
- docs/TEST-PLAN.md — Coverage goals, data model, coverage matrix

## AI Engineering

Built with Claude Code as primary engineering accelerator.
All architectural decisions are human-authored and documented in /docs.