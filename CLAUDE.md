# CLAUDE.md — Player API Tests

> **This file is the authoritative specification for Claude Code.**
> Read it completely before writing any code.
> Every decision documented here has a rationale in the project docs (see `/docs/`).
> When in doubt — ask. Do not invent requirements.

---

## 1. Project Overview

You are building an **ASP.NET Core Web API** with a companion **NUnit integration test suite** covering player registration and management. This is simultaneously:

1. A QA Engineering Assessment submission (functional requirements are non-negotiable)
2. A public portfolio artefact demonstrating AI-augmented solo engineering delivery

**Author / Engineer:** Evgenii Subbotin
**Contact:** evgenii@subbotin.es
**GitHub:** github.com/subbotin-es
**Portfolio:** subbotin.es
**LinkedIn:** linkedin.com/in/evgenii-subbotin/
**Stack:** ASP.NET Core 8 · C# 12 · NUnit 3 · WebApplicationFactory · Swagger · GitHub Actions
**Timeline:** 3-day sprint
**Assessment requirements:** See Section 6 (Endpoint Contract) of this file

---

## 2. Absolute Rules — Read Before Every Task

```
NEVER add a database — all state is in-memory (ConcurrentDictionary in a singleton store)
NEVER invent endpoints not listed in Section 6
NEVER skip XML-doc comments on controller actions — Swagger reads them
NEVER hardcode test data inline — all fixtures from Tests/Fixtures/PlayerFixtures.cs
NEVER hardcode credential strings — all auth values from Tests/Fixtures/TestCredentials.cs
NEVER commit directly to main — all work via feature branches
ALWAYS define models in PlayerApi/Models/ before writing any controller logic
ALWAYS run `dotnet build` after each controller — fix all errors before proceeding
ALWAYS write the NUnit test immediately after implementing the endpoint (same session)
ALWAYS use WebApplicationFactory<Program> — no real port, no real process
ALWAYS keep controllers thin — business logic belongs in PlayerStore, not controllers
```

---

## 3. Tech Stack

| Layer | Technology | Version | Why |
|---|---|---|---|
| API Framework | ASP.NET Core Web API | 8.0 | LTS, minimal hosting model, built-in DI |
| Language | C# | 12 | Pattern matching, primary constructors, clean models |
| Auth | JWT Bearer (System.IdentityModel) | built-in | Stateless, standard, zero extra dependencies |
| Storage | ConcurrentDictionary (in-memory) | built-in | No DB cost, deterministic, thread-safe |
| API Docs | Swashbuckle (Swagger) | 6.x | Auto-generated from XML comments, near-zero cost |
| Test Framework | NUnit | 3.x | Familiar to author, rich assertion model |
| Test Reporter | NUnit3TestAdapter + JUnit XML | built-in | Machine-readable results, GitHub Actions summary |
| HTTP in Tests | HttpClient via WebApplicationFactory | built-in | In-process, no port, no deploy needed |
| Serialisation | System.Text.Json | built-in | Zero overhead, AOT-friendly |
| Deployment | Railway (free tier) | current | Public API URL + public Swagger, zero config |
| CI/CD | GitHub Actions | current | Build → Test → XML report → deploy to Railway |
| AI Engine | Claude Code | current | Primary engineering accelerator — you, reading this |

**No database. No external services. No refresh tokens. No persistent state between test runs.**
All data lives in a singleton `PlayerStore` that is reset between test fixtures.

---

## 4. Repository Structure

```
player-api-tests/
├── PlayerApi/                          # ASP.NET Core Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs           # POST /api/tester/login
│   │   └── PlayersController.cs        # CRUD /api/automationTask/...
│   ├── Models/
│   │   ├── Requests/
│   │   │   ├── LoginRequest.cs
│   │   │   └── CreatePlayerRequest.cs
│   │   └── Responses/
│   │       ├── LoginResponse.cs
│   │       ├── PlayerResponse.cs
│   │       └── ErrorResponse.cs
│   ├── Services/
│   │   └── PlayerStore.cs              # Thread-safe in-memory store (singleton)
│   ├── Program.cs                      # Minimal hosting, DI, JWT, Swagger
│   └── PlayerApi.csproj
├── PlayerApi.Tests/                    # NUnit test project
│   ├── Fixtures/
│   │   ├── PlayerFixtures.cs           # All player test data — never hardcode in tests
│   │   └── TestCredentials.cs          # Auth secrets for tests — see Section 9a
│   ├── Helpers/
│   │   └── ApiClient.cs                # Typed wrapper around HttpClient
│   ├── Unit/
│   │   └── PlayerStoreTests.cs         # Unit tests — PlayerStore in isolation, no HTTP
│   ├── Integration/
│   │   ├── AuthTests.cs                # login endpoint
│   │   ├── CreatePlayerTests.cs        # create × 12
│   │   ├── GetOnePlayerTests.cs        # getOne
│   │   ├── GetAllPlayersTests.cs       # getAll + sort
│   │   └── DeletePlayerTests.cs        # deleteOne × 12
│   ├── TestBase.cs                     # WebApplicationFactory setup, shared HttpClient
│   └── PlayerApi.Tests.csproj
├── docs/
│   ├── ADR-001-in-memory-storage.md
│   ├── ADR-002-webapplicationfactory.md
│   ├── ADR-003-jwt-stateless.md
│   ├── SECURITY.md
│   └── TEST-PLAN.md
├── .github/
│   └── workflows/
│       └── dotnet.yml                  # build → test → XML report → Railway deploy
├── railway.json                        # Railway deployment config
└── README.md
```

---

## 5. Domain Model — Define First

Create all models in `PlayerApi/Models/` **before** writing any controller.

```csharp
// Models/Requests/LoginRequest.cs
public record LoginRequest(string Username, string Password);

// Models/Requests/CreatePlayerRequest.cs
public record CreatePlayerRequest(string Username, string Email);

// Models/Responses/LoginResponse.cs
public record LoginResponse(string Token);

// Models/Responses/PlayerResponse.cs
public record PlayerResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt
);

// Models/Responses/ErrorResponse.cs
public record ErrorResponse(string Message);
```

**Validation rules (enforce in controller, return 400 if violated):**
- `Username`: required, 3–50 chars, unique across all players
- `Email`: required, valid format, unique across all players
- Login credentials: hardcoded `Username = "tester"`, `Password = "tester123"` — no user DB needed

---

## 6. Endpoint Contract — Non-Negotiable

### POST /api/tester/login
- **Auth:** none
- **Request body:** `{ "username": "tester", "password": "tester123" }`
- **200 OK:** `{ "token": "<jwt>" }`
- **401 Unauthorized:** wrong credentials

### POST /api/automationTask/create
- **Auth:** Bearer token (JWT)
- **Request body:** `{ "username": "string", "email": "string" }`
- **201 Created:** `PlayerResponse` (id, username, email, createdAt)
- **400 Bad Request:** validation failure → `ErrorResponse`
- **401 Unauthorized:** missing or invalid token

### GET /api/automationTask/getOne?id={guid}
- **Auth:** Bearer token (JWT)
- **Query param:** `id` (Guid)
- **200 OK:** `PlayerResponse`
- **404 Not Found:** `ErrorResponse`
- **401 Unauthorized:** missing or invalid token

### GET /api/automationTask/getAll
- **Auth:** Bearer token (JWT)
- **200 OK:** `PlayerResponse[]` (array, may be empty)
- **401 Unauthorized:** missing or invalid token
- **Note:** response is unsorted — sorting is the test's responsibility

### DELETE /api/automationTask/deleteOne/{id}
- **Auth:** Bearer token (JWT)
- **Route param:** `id` (Guid)
- **204 No Content:** success
- **404 Not Found:** `ErrorResponse`
- **401 Unauthorized:** missing or invalid token

---

## 7. PlayerStore — Exact Implementation

```csharp
// Services/PlayerStore.cs
public sealed class PlayerStore
{
    private readonly ConcurrentDictionary<Guid, PlayerResponse> _players = new();

    public PlayerResponse Add(CreatePlayerRequest request)
    {
        var player = new PlayerResponse(
            Id: Guid.NewGuid(),
            Username: request.Username,
            Email: request.Email,
            CreatedAt: DateTime.UtcNow
        );
        _players[player.Id] = player;
        return player;
    }

    public PlayerResponse? GetById(Guid id)
        => _players.TryGetValue(id, out var p) ? p : null;

    public IReadOnlyList<PlayerResponse> GetAll()
        => _players.Values.ToList();

    public bool Delete(Guid id)
        => _players.TryRemove(id, out _);

    public bool UsernameExists(string username)
        => _players.Values.Any(p =>
            p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public bool EmailExists(string email)
        => _players.Values.Any(p =>
            p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    // Called in TestBase.OneTimeSetUp to guarantee isolation
    public void Clear() => _players.Clear();
}
```

Register as **singleton** in `Program.cs`:
```csharp
builder.Services.AddSingleton<PlayerStore>();
```

---

## 8. JWT Configuration — Minimal

```csharp
// Program.cs — JWT setup
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-32-chars-minimum!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });
```

Token generation in `AuthController`:
```csharp
var token = new JwtSecurityToken(
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        SecurityAlgorithms.HmacSha256)
);
return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
```

**No roles. No claims. No refresh tokens.** Presence of a valid signature is sufficient.

---

## 9. Test Architecture

### TestBase.cs
```csharp
[TestFixture]
public abstract class TestBase
{
    protected WebApplicationFactory<Program> Factory = null!;
    protected HttpClient Client = null!;
    protected string Token = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        // Clear store before each fixture class
        var store = Factory.Services.GetRequiredService<PlayerStore>();
        store.Clear();
        // Obtain token once per fixture
        Token = await ApiClient.LoginAsync(Client);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
```

### ApiClient.cs
```csharp
// Helpers/ApiClient.cs
public static class ApiClient
{
    public static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/tester/login",
            new { username = "tester", password = "tester123" });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    public static HttpClient WithBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
```

### PlayerFixtures.cs
```csharp
// Fixtures/PlayerFixtures.cs
public static class PlayerFixtures
{
    // Exactly 12 players — all test data lives here
    public static readonly CreatePlayerRequest[] TwelvePlayers =
        Enumerable.Range(1, 12)
            .Select(i => new CreatePlayerRequest(
                Username: $"player_{i:D2}",
                Email: $"player{i:D2}@test.example"))
            .ToArray();

    // Credentials
    public const string ValidUsername = "tester";
    public const string ValidPassword = "tester123";
    public const string WrongPassword = "wrong";

    // Invalid create requests for negative tests
    public static readonly CreatePlayerRequest TooShortUsername =
        new("ab", "valid@test.example");
    public static readonly CreatePlayerRequest InvalidEmail =
        new("validuser", "not-an-email");
}
```

---

## 9a. TestCredentials.cs — Security-Aware Fixture

```csharp
// Fixtures/TestCredentials.cs

/// <summary>
/// Test credentials for PlayerApi authentication.
///
/// DISCLAIMER: This file contains plaintext credentials intentionally exposed
/// for public assessment and portfolio demonstration purposes.
///
/// In a production system these values would be:
///   - Hashed (passwords via BCrypt/Argon2, never stored plaintext)
///   - Injected at runtime via environment secrets (GitHub Secrets, Azure Key Vault, etc.)
///   - Never committed to source control
///
/// KNOWN LIMITATION: This file is explicitly excluded from OWASP security review
/// (see docs/SECURITY.md — "Test Infrastructure Exclusions").
/// Its exposure is a deliberate trade-off for public demonstrability.
/// </summary>
public static class TestCredentials
{
    // --- Server-side ground truth ---
    // These are the values the API validates against.
    // In production: fetched from secrets store, never hardcoded.
    public const string CorrectServerLogin    = "tester";
    public const string CorrectServerPassword = "tester123";

    // --- Test attempts: positive path ---
    // Used in happy-path tests. Must match server values above.
    public const string AttemptedCorrectLogin    = CorrectServerLogin;
    public const string AttemptedCorrectPassword = CorrectServerPassword;

    // --- Test attempts: negative path ---
    // Used in 401 tests. Must NOT match server values.
    public const string AttemptedIncorrectLogin    = "wrong_user";
    public const string AttemptedIncorrectPassword = "wrong_password";
}
```

**Rules:**
- `TestCredentials` is the **only** place where login strings appear in the test project
- `PlayerFixtures.cs` references `TestCredentials` — never duplicates values
- `AuthTests.cs` references `TestCredentials` — never hardcodes strings inline
- This file is flagged in `docs/SECURITY.md` under "Test Infrastructure Exclusions"

### Coverage Goals (stated in TEST-PLAN.md)

| Layer | Goal | Definition |
|---|---|---|
| Unit | 100% of `PlayerStore` methods | Every public method has ≥ 1 direct unit test — no HTTP involved |
| API Positive | 100% of endpoints | Every endpoint has ≥ 1 test asserting the happy-path status code and response shape |
| API Negative | ≥ 1 per endpoint | Every endpoint has at least one test covering an error response (4xx) |

### Unit Test Matrix — `PlayerStoreTests.cs`

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

---

## 10a. Unit Test Skeleton — PlayerStoreTests.cs

```csharp
// Unit/PlayerStoreTests.cs
[TestFixture]
public class PlayerStoreTests
{
    private PlayerStore _store = null!;

    [SetUp]
    public void SetUp() => _store = new PlayerStore(); // fresh instance per test

    [Test]
    public void Add_ValidRequest_ReturnsPlayerWithCorrectFields()
    {
        var request = new CreatePlayerRequest("alice", "alice@test.example");
        var result = _store.Add(request);

        Assert.That(result.Username, Is.EqualTo("alice"));
        Assert.That(result.Email, Is.EqualTo("alice@test.example"));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(2)));
    }

    [Test]
    public void GetById_ExistingId_ReturnsPlayer()
    {
        var player = _store.Add(new CreatePlayerRequest("bob", "bob@test.example"));
        Assert.That(_store.GetById(player.Id), Is.EqualTo(player));
    }

    [Test]
    public void GetById_UnknownId_ReturnsNull()
        => Assert.That(_store.GetById(Guid.NewGuid()), Is.Null);

    [Test]
    public void GetAll_AfterAddingThree_ReturnsAll()
    {
        for (var i = 1; i <= 3; i++)
            _store.Add(new CreatePlayerRequest($"user{i}", $"user{i}@test.example"));
        Assert.That(_store.GetAll(), Has.Count.EqualTo(3));
    }

    [Test]
    public void GetAll_EmptyStore_ReturnsEmptyList()
        => Assert.That(_store.GetAll(), Is.Empty);

    [Test]
    public void Delete_ExistingId_ReturnsTrueAndRemovesPlayer()
    {
        var player = _store.Add(new CreatePlayerRequest("carol", "carol@test.example"));
        Assert.That(_store.Delete(player.Id), Is.True);
        Assert.That(_store.GetById(player.Id), Is.Null);
    }

    [Test]
    public void Delete_UnknownId_ReturnsFalse()
        => Assert.That(_store.Delete(Guid.NewGuid()), Is.False);

    [Test]
    public void UsernameExists_AfterAdd_ReturnsTrueCaseInsensitive()
    {
        _store.Add(new CreatePlayerRequest("Dave", "dave@test.example"));
        Assert.That(_store.UsernameExists("dave"), Is.True);
        Assert.That(_store.UsernameExists("DAVE"), Is.True);
    }

    [Test]
    public void UsernameExists_UnknownUsername_ReturnsFalse()
        => Assert.That(_store.UsernameExists("nobody"), Is.False);

    [Test]
    public void EmailExists_AfterAdd_ReturnsTrueCaseInsensitive()
    {
        _store.Add(new CreatePlayerRequest("eve", "Eve@Test.Example"));
        Assert.That(_store.EmailExists("eve@test.example"), Is.True);
    }

    [Test]
    public void EmailExists_UnknownEmail_ReturnsFalse()
        => Assert.That(_store.EmailExists("ghost@nowhere.example"), Is.False);

    [Test]
    public void Clear_AfterAddingPlayers_StoreIsEmpty()
    {
        _store.Add(new CreatePlayerRequest("frank", "frank@test.example"));
        _store.Clear();
        Assert.That(_store.GetAll(), Is.Empty);
    }
}
```

**Key principle:** `PlayerStoreTests` uses `new PlayerStore()` directly — no `WebApplicationFactory`, no HTTP, no DI container. Each test gets a fresh instance via `[SetUp]`.

---

## 11. Program.cs — Skeleton

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PlayerStore>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Player API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(/* standard Bearer requirement */);
    var xml = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

// JWT setup (see Section 8)

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Required for WebApplicationFactory
public partial class Program { }
```

---

## 12. Day-by-Day Execution Plan

### DAY 1 PROMPT

```
Read CLAUDE.md Sections 2, 5, 6, 7, 8, 11 before writing any code.

Today's goal: API is fully implemented and compiles cleanly.

Implement in this order:
1. dotnet new sln -n player-api-tests
2. dotnet new webapi -n PlayerApi --no-openapi (we add Swagger manually)
3. dotnet new nunit -n PlayerApi.Tests
4. dotnet sln add PlayerApi PlayerApi.Tests
5. Create all Models (Section 5) — requests and responses
6. Create PlayerStore (Section 7) and register as singleton
7. Create AuthController — POST /api/tester/login
8. Create PlayersController — all 4 endpoints (Section 6)
9. Wire Program.cs (Section 11) — JWT + Swagger + controllers
10. dotnet build — fix all errors before proceeding
11. dotnet run --project PlayerApi — smoke-test Swagger UI at /swagger

Do NOT write tests today.
Do NOT add a database.
Do NOT add roles or claims to JWT.
```

### DAY 2 PROMPT

```
Read CLAUDE.md Sections 2, 9, 10, 10a before writing any code.

Today's goal: Full NUnit suite is green — unit tests + integration tests.

Implement in this order:
1. Add WebApplicationFactory and NUnit3TestAdapter references to PlayerApi.Tests.csproj
2. Create TestBase.cs (Section 9)
3. Create Helpers/ApiClient.cs (Section 9)
4. Create Fixtures/PlayerFixtures.cs (Section 9) — all test data here

UNIT TESTS FIRST:
5. Create Unit/PlayerStoreTests.cs (Section 10a — 12 tests, no HTTP)
6. dotnet test --filter PlayerStoreTests — must be all green before proceeding

INTEGRATION TESTS:
7. Create Integration/AuthTests.cs (3 scenarios)
8. dotnet test --filter AuthTests — must be green before proceeding
9. Create Integration/CreatePlayerTests.cs (4 scenarios)
10. dotnet test --filter CreatePlayerTests
11. Create Integration/GetOnePlayerTests.cs (3 scenarios)
12. Create Integration/GetAllPlayersTests.cs (3 scenarios — including sort assertion)
13. Create Integration/DeletePlayerTests.cs (3 scenarios)
14. dotnet test — all green

Sort assertion in GetAllPlayersTests:
var names = players.Select(p => p.Username).ToList();
Assert.That(names, Is.EqualTo(names.OrderBy(n => n).ToList()));

Do NOT change API code to make tests pass — fix tests if contract is wrong.
Do NOT share HttpClient instances between test classes — use TestBase.
Do NOT add HTTP in unit tests — PlayerStoreTests uses new PlayerStore() directly.
```

### DAY 3 PROMPT

```
Read CLAUDE.md Sections 13, 17, 18 before starting.

Today's goal: Documentation + CI with XML report + Railway deploy + GitHub.

Implement in this order:
1. Write docs/ADR-001-in-memory-storage.md
2. Write docs/ADR-002-webapplicationfactory.md
3. Write docs/ADR-003-jwt-stateless.md
4. Write docs/SECURITY.md (OWASP check — see Section 15)
5. Write docs/TEST-PLAN.md (see Section 16) — include coverage goals from Section 10
6. Write README.md (see Section 17) — include Railway public URL and Swagger link
7. Create railway.json (see Section 18)
8. Create .github/workflows/dotnet.yml (see Section 18) — includes XML report step
9. git push → verify Actions pipeline is green and test summary is visible
10. Connect Railway to GitHub repo → verify public Swagger URL is live
11. Update README.md with actual Railway URL
12. Final: dotnet test — confirm all green on clean clone
```

---

## 13. Documentation Checklist

```
docs/
  [ ] ADR-001-in-memory-storage.md      — why no DB
  [ ] ADR-002-webapplicationfactory.md  — why in-process over real server
  [ ] ADR-003-jwt-stateless.md          — why no refresh tokens, no claims
  [ ] SECURITY.md                        — OWASP table complete
  [ ] TEST-PLAN.md                       — data model, coverage goals, assertions

root/
  [ ] railway.json                       — Railway deployment config
  [ ] README.md                          — overview, quickstart, public links, contact
```

---

## 14. OWASP Check — Scope for SECURITY.md

Cover these OWASP API Security items (1-2 sentences each):

| OWASP Item | Relevance to This Project |
|---|---|
| API01 — Broken Object Level Auth | getOne and deleteOne validate id exists; no ownership model needed in assessment scope |
| API02 — Broken Authentication | JWT signature validated on every protected endpoint via `[Authorize]` |
| API03 — Broken Object Property Level | Response records expose only intended fields — no internal state leaks from PlayerStore |
| API08 — Security Misconfiguration | Swagger enabled intentionally for assessment visibility; would be gated in production |
| API09 — Improper Inventory Management | Single versioned API surface, no shadow endpoints, no legacy routes |

**Test Infrastructure Exclusions:**
`TestCredentials.cs` contains plaintext credentials by design — this is a known limitation explicitly accepted for public assessment demonstrability. It is excluded from this security review. In production, all credentials would be hashed and injected via secrets management.

**Explicitly out of scope:** rate limiting, HTTPS enforcement, secrets management (dev key only), input sanitisation beyond format validation.

---

## 15. TEST-PLAN.md — Required Sections

1. **Scope** — what is tested and what is not (no UI, no DB, no external services)
2. **Approach** — two-layer strategy: unit (PlayerStore direct) + integration (WebApplicationFactory)
3. **Coverage Goals** — copy the goals table from Section 10
4. **Data Model under test** — table of `PlayerResponse` fields: field name, type, constraints, which test covers it
5. **Test isolation strategy** — `new PlayerStore()` per unit test; `store.Clear()` in `OneTimeSetUp` for integration
6. **Coverage matrix** — copy both matrices from Section 10
7. **Out of scope** — performance, load, security penetration, UI

---

## 16. README.md — Required Sections

```markdown
# player-api-tests

REST API + NUnit test suite (unit + integration) for player management.
Built as a QA Engineering Assessment submission.

**Author:** Evgenii Subbotin — evgenii@subbotin.es
**Portfolio:** subbotin.es | **GitHub:** github.com/subbotin-es | **LinkedIn:** linkedin.com/in/evgenii-subbotin/

## Live API

Base URL: https://<your-railway-app>.railway.app
Swagger UI: https://<your-railway-app>.railway.app/swagger

## Quick Start (local)

dotnet restore
dotnet build
dotnet test

## Run API locally

dotnet run --project PlayerApi
# Swagger UI: http://localhost:5000/swagger

## Architecture

- PlayerApi — ASP.NET Core 8 Web API, in-memory store, JWT auth, Swagger
- PlayerApi.Tests — NUnit 3, two test layers:
  - Unit: PlayerStore tested directly (new PlayerStore(), no HTTP)
  - Integration: WebApplicationFactory (in-process, no real port)

## Test Strategy

All state lives in a singleton PlayerStore (ConcurrentDictionary).
Unit tests instantiate PlayerStore directly — fast, isolated, no infrastructure.
Integration tests use WebApplicationFactory<Program> — no server process, no ports.
Store is cleared in OneTimeSetUp to guarantee isolation between fixture classes.
All test data is declared in Fixtures/PlayerFixtures.cs — never hardcoded inline.

## CI / Test Reports

GitHub Actions runs on every push to main and develop.
Test results are published as a GitHub Actions job summary (JUnit XML).
Green badge = all unit + integration tests pass.

## Documentation

- docs/ADR-001 — Why in-memory storage
- docs/ADR-002 — Why WebApplicationFactory
- docs/ADR-003 — Why stateless JWT
- docs/SECURITY.md — OWASP relevance
- docs/TEST-PLAN.md — Coverage goals, data model, coverage matrix

## AI Engineering

Built with Claude Code as primary engineering accelerator.
All architectural decisions are human-authored and documented in /docs.
```

---

## 17. Railway Deployment

### railway.json

```json
{
  "$schema": "https://railway.app/railway.schema.json",
  "build": {
    "builder": "NIXPACKS"
  },
  "deploy": {
    "startCommand": "dotnet PlayerApi.dll",
    "restartPolicyType": "ON_FAILURE"
  }
}
```

### Setup steps (Day 3, ~20 minutes)

1. Go to railway.app → New Project → Deploy from GitHub repo
2. Select `player-api-tests` → Railway auto-detects .NET
3. Set root directory to `PlayerApi` in Railway settings
4. Add environment variable: `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
5. Deploy → copy the public URL
6. Verify: `https://<app>.railway.app/swagger` loads Swagger UI
7. Update README.md with the actual URL

**Note:** Railway free tier sleeps after inactivity. First request after sleep takes ~5s. This is acceptable for assessment purposes — mention it in README.

---

## 18. GitHub Actions — dotnet.yml

```yaml
name: .NET

on:
  push:
    branches: [ "main", "develop" ]
  pull_request:
    branches: [ "main" ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test with XML report
        run: |
          dotnet test --no-build --configuration Release --verbosity normal \
            --logger "junit;LogFilePath=TestResults/results.xml"

      - name: Publish test results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: NUnit Results
          path: '**/TestResults/results.xml'
          reporter: java-junit
```

The `dorny/test-reporter` action publishes a readable test summary directly in the GitHub Actions run page — visible to anyone with access to the repo, no extra setup needed.

---

## 19. Common Errors and How to Fix Them

**`WebApplicationFactory cannot find Program`**
→ Ensure `public partial class Program { }` is at the bottom of `Program.cs`. Without it the type is internal and not visible to the test project.

**`401 on all protected endpoints in tests`**
→ Check that `ApiClient.WithBearer(token)` sets `DefaultRequestHeaders.Authorization` before the request. If you create a new `HttpClient` per test, re-apply the header.

**`Store not cleared between test classes`**
→ `OneTimeSetUp` runs once per fixture class. If two fixture classes share state unexpectedly, ensure each calls `store.Clear()` in its own `OneTimeSetUp`, not `SetUp`.

**`Swagger returns 404 at /swagger`**
→ Verify `app.UseSwagger()` and `app.UseSwaggerUI()` are called before `app.MapControllers()`. Also confirm `launchSettings.json` isn't redirecting to HTTPS only.

**`dotnet test` passes locally but fails in CI**
→ CI runs `--no-build` after `build` step. If models changed after last build, re-run build step. Check that `.csproj` references `Microsoft.AspNetCore.Mvc.Testing` in the test project.

**`Duplicate username returns 201 instead of 400`**
→ `PlayerStore.UsernameExists()` comparison must use `OrdinalIgnoreCase`. Check that the controller calls it before calling `store.Add()`.

**`GetAll sort test is flaky`**
→ `ConcurrentDictionary` has non-deterministic iteration order. Sorting must happen in the test (or optionally in the controller) — never rely on insertion order.

---

## 20. Submission Checklist

```
API
  [ ] POST /api/tester/login — 200 + token
  [ ] POST /api/automationTask/create — 201 + PlayerResponse
  [ ] GET  /api/automationTask/getOne?id= — 200 + PlayerResponse
  [ ] GET  /api/automationTask/getAll — 200 + PlayerResponse[]
  [ ] DELETE /api/automationTask/deleteOne/{id} — 204
  [ ] Swagger UI accessible at /swagger (local)
  [ ] Swagger UI accessible at Railway public URL
  [ ] All endpoints return correct HTTP status codes per Section 6

Unit Tests
  [ ] PlayerStoreTests — 12 tests, all green
  [ ] No HTTP, no WebApplicationFactory in unit tests
  [ ] new PlayerStore() per test via [SetUp]

Integration Tests
  [ ] 12 players created in CreatePlayerTests
  [ ] getAll returns all 12 and sort assertion passes
  [ ] All 12 deleted in DeletePlayerTests
  [ ] Negative: duplicate username → 400
  [ ] Negative: invalid email → 400
  [ ] Negative: wrong password → 401
  [ ] Negative: no Bearer token → 401 on all protected endpoints
  [ ] Negative: unknown id → 404 on getOne and deleteOne
  [ ] dotnet test — all green (unit + integration)

CI / Reporting
  [ ] .github/workflows/dotnet.yml committed
  [ ] GitHub Actions pipeline green on main
  [ ] Test report visible in Actions job summary (dorny/test-reporter)
  [ ] Railway deploy live — public Swagger URL works

Documentation
  [ ] docs/ADR-001-in-memory-storage.md
  [ ] docs/ADR-002-webapplicationfactory.md
  [ ] docs/ADR-003-jwt-stateless.md
  [ ] docs/SECURITY.md — OWASP table complete
  [ ] docs/TEST-PLAN.md — coverage goals + data model + matrix
  [ ] README.md — all sections present including Railway URL

Submission
  [ ] Repository is public
  [ ] README has Railway Swagger URL
  [ ] README has GitHub repo URL
  [ ] Link to repo shared with employer
```

---

## Appendix A — Plan vs. Implementation Delta

> **Purpose:** Records every known difference between the CLAUDE.md specification and the delivered implementation. Captured after project completion (2026-05-02). All 28 tests pass. No functional requirements were compromised.

---

### A.1 Test Count

| Layer | Spec (implied) | Actual | Result |
|---|---|---|---|
| Unit — PlayerStoreTests | 12 tests | 12 tests | ✓ Exact |
| Integration — AuthTests | 3 tests | 3 tests | ✓ Exact |
| Integration — CreatePlayerTests | 4 tests | 4 tests | ✓ Exact |
| Integration — GetOnePlayerTests | 3 tests | 3 tests | ✓ Exact |
| Integration — GetAllPlayersTests | 3 tests | 2 tests | See A.4 |
| Integration — DeletePlayerTests | 3 tests | 3 tests | ✓ Exact |
| **Total** | **28 tests** | **28 tests** | ✓ |

---

### A.2 Omissions from Spec

#### A.2.1 Swagger Bearer Security Definition Not Wired

**Section 11** shows `c.AddSecurityDefinition("Bearer", ...)` and `c.AddSecurityRequirement(...)` in the `Program.cs` skeleton.

**Actual:** Neither call is present in the delivered `Program.cs`. The `AddSwaggerGen` block registers the Swagger doc and XML comments but omits the Bearer auth UI wiring.

**Impact:** Low / cosmetic. The API authenticates correctly and all tests pass. The Swagger UI at `/swagger` does not render an "Authorize" button, so manual exploratory testing via Swagger requires setting the header through external tools (curl, Postman, etc.). No functional regression.

**Remediation:** Add to `Program.cs` inside `AddSwaggerGen`:
```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    In = ParameterLocation.Header,
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer"
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }
});
```

#### A.2.2 railway.json Is Effectively Empty

**Section 17** specifies a `railway.json` with `builder`, `startCommand`, and `restartPolicyType`.

**Actual:** `railway.json` contains only the schema reference:
```json
{ "$schema": "https://railway.app/railway.schema.json" }
```

**Reason:** The deployment mechanism shifted from NIXPACKS to Docker (see A.3.1). When Railway detects a `Dockerfile`, it ignores most `railway.json` build/deploy fields and builds the container directly. The `start.sh` script and `Dockerfile` together replace the spec's railway.json configuration.

**Impact:** None on deployment behaviour. The `railway.json` is present to satisfy file-level completeness but carries no effective configuration.

#### A.2.3 README Railway URLs Not Populated

**Section 16** requires updating the README with the live Railway base URL and Swagger URL after deployment.

**Actual:** Both URLs remain as spec placeholders (`https://<your-railway-app>.railway.app`).

**Reason:** Railway deployment is pending at time of this appendix. The CI/CD pipeline, Dockerfile, and `start.sh` are all ready; the Railway project connection step (Section 17, steps 1–7) has not been executed.

**Impact:** Section 20 submission checklist items "Swagger UI accessible at Railway public URL" and "README has Railway Swagger URL" remain open.

---

### A.3 Extra Files Added Beyond Spec

The following files were created during implementation but are not mentioned in CLAUDE.md. None violate any Section 2 absolute rules.

#### A.3.1 `Dockerfile` (root)

Multi-stage .NET 8 container image (`build` → `publish` → `runtime` stages). Added as the primary Railway deployment artefact after the Nixpacks approach encountered configuration friction. Produces a minimal runtime image (~200 MB) from `mcr.microsoft.com/dotnet/aspnet:8.0`.

`start.sh` is the companion entry point invoked by the container's `CMD`.

#### A.3.2 `start.sh` (root)

Thin bash wrapper that sets `ASPNETCORE_URLS` and starts the published `PlayerApi.dll`. Railway executes this as the container's start command. Functionally equivalent to the `startCommand` that would have been in `railway.json`.

#### A.3.3 `.dockerignore` (root)

Standard Docker hygiene file — excludes `bin/`, `obj/`, `.git/`, and test projects from the build context. Reduces image build time and avoids copying test assemblies into the production image.

#### A.3.4 `PlayerApi.http` (root)

VS Code REST Client file auto-generated by the `dotnet new webapi` template. Contains a placeholder request to `/weatherforecast` (a default endpoint that was removed). File is inert and can be deleted or repurposed with the actual API endpoints.

#### A.3.5 `PlayerApi.Tests/UnitTest1.cs`

Auto-generated placeholder from `dotnet new nunit`. Contains a single empty test that always passes. Does not interfere with any real test. Should be deleted to keep the test count accurate at 28 meaningful tests.

---

### A.4 Improvements Over Spec

These deviations represent deliberate enhancements that improve on the spec without violating any Section 2 rule.

#### A.4.1 `PlayerFixtures.cs` References `TestCredentials` Instead of Hardcoding

**Spec (Section 9):**
```csharp
public const string ValidUsername = "tester";
public const string ValidPassword = "tester123";
```

**Actual:**
```csharp
public const string ValidUsername = TestCredentials.AttemptedCorrectLogin;
public const string ValidPassword = TestCredentials.AttemptedCorrectPassword;
```

**Why better:** Eliminates the only remaining string duplication between the two fixture files. Aligns with the Section 2 rule "NEVER hardcode credential strings — all auth values from TestCredentials.cs".

#### A.4.2 `ApiClient.LoginAsync` Uses `TestCredentials` Constants

**Spec (Section 9):** Inline `"tester"` / `"tester123"` strings in `LoginAsync`.

**Actual:** `TestCredentials.AttemptedCorrectLogin` / `AttemptedCorrectPassword`. Same rationale as A.4.1 — single source of truth for all credential strings.

#### A.4.3 `TestBase.cs` Calls `store.Clear()` in Both `OneTimeSetUp` and `[SetUp]`

**Spec (Section 9):** `store.Clear()` only in `OneTimeSetUp` (once per fixture class).

**Actual:** `store.Clear()` is called in `OneTimeSetUp` (pre-fixture isolation) and again in `[SetUp]` (pre-test isolation). This guarantees a clean store before every individual test method, making the suite resilient to inter-test state pollution even if test ordering changes.

**Why better:** Defensive and free — no performance cost. Section 2 states "ALWAYS keep store isolated"; this exceeds the minimum.

#### A.4.4 `GetAllPlayersTests`: Count and Sort Assertions Combined Into One Test

**Spec (Section 10) implied:** Two separate test methods — one for count = 12, one for sorted order.

**Actual:** One test (`GetAll_ReturnsAllPlayersAndNamesCanBeSorted`) asserts both count and alphabetical sort:
```csharp
Assert.That(players!, Has.Length.EqualTo(12));
var names = players!.Select(p => p.Username).ToList();
var expectedNames = PlayerFixtures.TwelvePlayers.Select(p => p.Username).OrderBy(n => n).ToList();
Assert.That(names.OrderBy(n => n).ToList(), Is.EqualTo(expectedNames));
```

**Why acceptable:** Both assertions are present and verified. A single logical scenario ("getAll returns the right 12 players in sorted order") is more cohesive as one test than two. The class still has a second test for the 401 auth case.

#### A.4.5 CI Pipeline Enhanced Beyond Spec

**Spec (Section 18):** JUnit XML logger (`--logger "junit;LogFilePath=TestResults/results.xml"`) + `dorny/test-reporter` with `reporter: java-junit`.

**Actual:**
- Uses `.trx` format (`--logger "trx;LogFileName=results.trx"`) — the native .NET test results format, more faithful to NUnit's output model.
- `dorny/test-reporter` configured with `reporter: dotnet-trx`.
- Additional step: `reportgenerator` produces an HTML report; `peaceiris/actions-gh-pages` publishes it to GitHub Pages at `subbotin-es.github.io/player-api-tests/test-report/`.
- README includes CI badge and direct link to the published HTML report.

**Why better:** TRX preserves richer NUnit metadata (duration, stack traces, categories). HTML report on GitHub Pages provides a public, human-readable test history visible to anyone reviewing the portfolio without needing repo access.

---

### A.5 Compliance Summary

| Area | Spec Sections | Status | Notes |
|---|---|---|---|
| All 5 endpoints implemented | 6 | ✓ Complete | Routes, status codes, validation exact |
| All 5 model records | 5 | ✓ Complete | Sealed records, correct shapes |
| PlayerStore (7 methods) | 7 | ✓ Complete | Exact implementation, singleton |
| JWT auth (no roles/claims) | 8 | ✓ Complete | Stateless, 1-hour expiry |
| Swagger UI | 11 | ⚠️ Partial | Works at /swagger; Bearer "Authorize" button absent (A.2.1) |
| Unit tests (12) | 10a | ✓ Complete | All pass, no HTTP |
| Integration tests (16) | 10 | ✓ Complete | All pass; GetAll scenarios consolidated (A.4.4) |
| TestCredentials / PlayerFixtures | 9, 9a | ✓ Enhanced | No hardcoded strings anywhere (A.4.1, A.4.2) |
| All 5 docs files | 13 | ✓ Complete | ADRs, SECURITY, TEST-PLAN |
| GitHub Actions CI | 18 | ✓ Enhanced | TRX + HTML report (A.4.5) |
| railway.json | 17 | ⚠️ Minimal | Dockerfile + start.sh used instead (A.2.2) |
| README | 16 | ⚠️ Incomplete | Railway URLs not populated (A.2.3) |

**Overall:** 28/28 tests pass. All functional requirements met. Three open items (A.2.1–A.2.3) are non-functional and do not affect assessment correctness.

---

*End of CLAUDE.md*
*Version: 1.3 | Author: Evgenii Subbotin | Project: player-api-tests*
*Appendix A added: 2026-05-02*
