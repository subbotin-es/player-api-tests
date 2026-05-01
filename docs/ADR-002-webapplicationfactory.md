# ADR-002: WebApplicationFactory for Testing

## Status
Accepted

## Context
Integration tests need to exercise the full ASP.NET Core pipeline (controllers, middleware, DI) without real HTTP ports or external processes.

## Decision
Use `WebApplicationFactory<Program>` for all integration tests.

## Rationale
- **In-process**: Tests run in the same process, no ports or deployment.
- **Fast**: No network overhead.
- **Isolated**: Each test fixture gets a fresh factory instance.
- **Realistic**: Exercises actual HTTP pipeline, not mocks.
- **Standard**: Microsoft's recommended approach for ASP.NET Core testing.

## Consequences
- **Positive**: Accurate, fast, isolated integration tests.
- **Negative**: Requires careful DI and configuration setup.
- **Mitigation**: Use `TestBase` with proper factory configuration.

## Alternatives Considered
- Real HTTP server: Slower, requires ports, harder to isolate.
- Full mocks: Less realistic, misses integration bugs.

## References
- CLAUDE.md Section 9: Test Architecture