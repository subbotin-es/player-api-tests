# Security Assessment

## OWASP API Security Top 10 Relevance

| OWASP Item | Relevance to This Project |
|---|---|
| API01 — Broken Object Level Auth | getOne and deleteOne validate id exists; no ownership model needed in assessment scope |
| API02 — Broken Authentication | JWT signature validated on every protected endpoint via `[Authorize]` |
| API03 — Broken Object Property Level | Response records expose only intended fields — no internal state leaks from PlayerStore |
| API08 — Security Misconfiguration | Swagger enabled intentionally for assessment visibility; would be gated in production |
| API09 — Improper Inventory Management | Single versioned API surface, no shadow endpoints, no legacy routes |

## Test Infrastructure Exclusions

`TestCredentials.cs` contains plaintext credentials intentionally exposed for public assessment and portfolio demonstration purposes.

In a production system these values would be:
- Hashed (passwords via BCrypt/Argon2, never stored plaintext)
- Injected at runtime via environment secrets (GitHub Secrets, Azure Key Vault, etc.)
- Never committed to source control

**KNOWN LIMITATION**: This file is explicitly excluded from OWASP security review (see CLAUDE.md — "Test Infrastructure Exclusions"). Its exposure is a deliberate trade-off for public demonstrability.

## Explicitly Out of Scope

- Rate limiting
- HTTPS enforcement
- Secrets management (dev key only)
- Input sanitisation beyond format validation

## Security Improvements Made

- JWT secret no longer hardcoded in source (Day 1 fix)
- Configuration required for JWT key
- Test isolation prevents state leakage

## References

- CLAUDE.md Section 14: OWASP Check Scope