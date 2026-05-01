# ADR-003: Stateless JWT Authentication

## Status
Accepted

## Context
The API needs authentication for protected endpoints, but refresh tokens and stateful sessions are overkill for this assessment.

## Decision
Use stateless JWT Bearer authentication with short-lived tokens (1 hour).

## Rationale
- **Stateless**: No server-side session storage.
- **Standard**: JWT is industry standard for APIs.
- **Simple**: No refresh logic, claims, or roles needed.
- **Secure enough**: Signature validation prevents tampering.
- **Testable**: Easy to generate tokens in tests.

## Consequences
- **Positive**: Lightweight, standard, easy to test.
- **Negative**: Tokens expire, no refresh mechanism.
- **Mitigation**: Short expiry for security, document limitations.

## Alternatives Considered
- API keys: Less standard, harder to manage.
- Stateful sessions: Adds complexity, not needed.

## References
- CLAUDE.md Section 8: JWT Configuration