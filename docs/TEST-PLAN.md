# Test Plan

## Scope

This test plan covers the Player API assessment submission. Tests are divided into unit and integration layers.

**What is tested:**
- PlayerStore business logic (unit)
- API endpoints with real HTTP (integration)
- Authentication and authorization
- Data validation and error handling

**What is not tested:**
- UI, performance, load, security penetration
- External services, databases, persistence

## Approach

Two-layer testing strategy:

1. **Unit Tests**: `PlayerStore` tested directly (new instance per test, no HTTP)
2. **Integration Tests**: Full pipeline via `WebApplicationFactory<Program>` (in-process, no real ports)

## Coverage Goals

| Layer | Goal | Definition |
|---|---|---|
| Unit | 100% of `PlayerStore` methods | Every public method has ≥ 1 direct unit test — no HTTP involved |
| API Positive | 100% of endpoints | Every endpoint has ≥ 1 test asserting the happy-path status code and response shape |
| API Negative | ≥ 1 per endpoint | Every endpoint has at least one test covering an error response (4xx) |

## Data Model Under Test

| Field | Type | Constraints | Coverage |
|---|---|---|---|
| Id | Guid | Auto-generated, unique | Tested in create and get operations |
| Username | string | 3-50 chars, unique (case-insensitive) | Validated in create, uniqueness in duplicate tests |
| Email | string | Valid email format, unique (case-insensitive) | Validated in create, uniqueness in duplicate tests |
| CreatedAt | DateTime | UTC, set on create | Verified in create responses |

## Test Isolation Strategy

- **Unit**: `new PlayerStore()` per test via `[SetUp]`
- **Integration**: `store.Clear()` in `TestBase.SetUp` per fixture class
- **JWT**: Environment variable injection for test host

## Coverage Matrix

### Unit Test Matrix — PlayerStoreTests.cs

| Method | Scenario | Assert |
|---|---|---|
| Add | Valid request | Returns PlayerResponse with correct Username, Email, non-empty Id, recent CreatedAt |
| GetById | Existing id | Returns correct player |
| GetById | Unknown id | Returns null |
| GetAll | After adding 3 players | Count = 3 |
| GetAll | Empty store | Returns empty list |
| Delete | Existing id | Returns true, subsequent GetById returns null |
| Delete | Unknown id | Returns false |
| UsernameExists | After adding player | Returns true (case-insensitive) |
| UsernameExists | Unknown username | Returns false |
| EmailExists | After adding player | Returns true (case-insensitive) |
| EmailExists | Unknown email | Returns false |
| Clear | After adding players | GetAll returns empty list |

### Integration Test Matrix

| Test Class | Scenario | Assert |
|---|---|---|
| AuthTests | Valid credentials | 200 + token non-empty |
| AuthTests | Wrong password | 401 |
| AuthTests | Missing body | 400 |
| CreatePlayerTests | Create 12 players sequentially | 201 + body matches request per player |
| CreatePlayerTests | Duplicate username | 400 |
| CreatePlayerTests | Invalid email format | 400 |
| CreatePlayerTests | No auth header | 401 |
| GetOnePlayerTests | Get existing player | 200 + correct player |
| GetOnePlayerTests | Unknown id | 404 |
| GetOnePlayerTests | No auth header | 401 |
| GetAllPlayersTests | Returns all 12 | 200 + count = 12 |
| GetAllPlayersTests | Sorted by username ascending | array order matches OrderBy |
| GetAllPlayersTests | No auth header | 401 |
| DeletePlayerTests | Delete all 12 | 204 per player |
| DeletePlayerTests | Delete same id twice | 404 on second call |
| DeletePlayerTests | No auth header | 401 |

## Out of Scope

- Performance, load, security penetration, UI
- Multi-threading beyond ConcurrentDictionary
- External integrations

## References

- CLAUDE.md Section 10: Test Architecture and Coverage Goals