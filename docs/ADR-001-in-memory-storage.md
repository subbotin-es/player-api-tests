# ADR-001: In-Memory Storage

## Status
Accepted

## Context
This project is a portfolio artefact and QA engineering assessment. It requires a simple, deterministic data store for player management without the complexity of external databases.

## Decision
Use `ConcurrentDictionary<Guid, PlayerResponse>` as an in-memory singleton store (`PlayerStore`).

## Rationale
- **Zero external dependencies**: No DB setup, migrations, or connection strings.
- **Thread-safe**: `ConcurrentDictionary` handles concurrent access.
- **Deterministic**: State is reset between test runs via `Clear()`.
- **Fast**: No I/O overhead for unit tests.
- **Simple**: Fits the assessment scope perfectly.

## Consequences
- **Positive**: Easy to test, no persistence, fast development.
- **Negative**: Data lost on restart, not suitable for production.
- **Mitigation**: Document as portfolio/demo only.

## Alternatives Considered
- Entity Framework with SQLite: Too heavy for assessment.
- Real database: Overkill, adds complexity.

## References
- CLAUDE.md Section 7: PlayerStore implementation