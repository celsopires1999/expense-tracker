## Context

The ExpenseTracker solution is a modular monolith with 3 bounded contexts (Auth, Expense, Permission), each with its own PostgreSQL database, EF Core DbContext, and MassTransit outbox. Currently only ArchitectureTests exist (6 NetArchTest layer dependency rules). There are zero tests for business logic, endpoint behavior, or cross-service communication.

All 3 services use:
- PostgreSQL via Npgsql with snake_case naming
- MassTransit with EF Core outbox + InMemory transport
- JWT Bearer authentication (Auth issues tokens, Expense/Permission validate via JWKS)
- Scrutor-based `IEndpoint` registration (no controllers)
- `Result<T>` pattern mapped to HTTP via `CustomResults.Problem`

The Expense service calls the Permission service via `PermissionServiceClient` (typed `HttpClient`) to resolve role-based permissions. The `/permissions/resolve` endpoint is `AllowAnonymous`.

Docker socket is shared from the host (DooD pattern in devcontainer). Testcontainers can run inside the container.

## Goals / Non-Goals

**Goals:**
- Integration tests against real PostgreSQL using Testcontainers (postgres:17)
- Level 2 tests: handler-level, direct invocation, no HTTP
- Level 3 tests: endpoint-level via `WebApplicationFactory<TEntryPoint>` (in-process TestServer)
- Real Permission service for Expense tests via `WebApplicationFactory` (in-process, no Docker)
- Domain event outbox verification (fix `SaveChangesAsync` to publish before DB commit)
- Database cleanup via TRUNCATE between test classes
- Parallel execution disabled per assembly to prevent race conditions

**Non-Goals:**
- Unit tests with mocked dependencies (Level 1)
- End-to-end tests with real HTTP between services
- RabbitMQ/broker testing (InMemory transport continues)
- Performance/load testing
- Test coverage reporting (can be added later via coverlet)

## Decisions

### D1: Shared PostgreSQL container via static lazy singleton

**Decision**: One `PostgreSqlContainer` instance shared across all test assemblies via a static `Lazy<T>` in `PostgreSqlFixture`.

**Why**: Multiple containers waste resources and slow CI. The static lazy pattern ensures the container starts once regardless of how many test assemblies reference `TestShared`.

**Alternatives considered**:
- Per-assembly container: 3 containers running simultaneously. Wasteful.
- `ICollectionFixture<T>` per assembly: xUnit v2 collection fixtures are assembly-scoped — won't share across assemblies.
- External PostgreSQL: contradicts the Testcontainers goal; requires pre-existing infrastructure.

### D2: RSA key generation per test run

**Decision**: Generate an ephemeral RSA 2048-bit key pair per test run, write temp PEM files, clean up on dispose.

**Why**: Keys must match between JWT signing (TokenProvider) and verification (AddAuthentication + JWKS endpoint). Generating per-run avoids committing test keys to the repo and ensures uniqueness across CI runs.

**Alternatives considered**:
- Fixed test keys in repo: simpler, but keys in source control are a security smell.
- In-memory key injection: would require modifying `TokenProvider` and `AddAuthentication` to accept RSA instances instead of file paths — too invasive.

### D3: WebApplicationFactory for Permission service (not Docker)

**Decision**: Run Permission.Api in-process via `WebApplicationFactory<Program>` inside the Expense test fixture.

**Why**: The only endpoint Expense calls is `POST /permissions/resolve` which is `AllowAnonymous`. No JWT validation needed. In-process is faster, simpler, and doesn't require a Dockerfile.

**Alternatives considered**:
- Docker container via Testcontainers: requires creating a Dockerfile for Permission.Api, managing JWT/JWKS configuration, slower startup.
- Mock `IPermissionServiceClient`: contradicts the "real dependency" requirement.

### D4: TRUNCATE + re-seed for database cleanup

**Decision**: Between test classes, TRUNCATE all domain tables with CASCADE and re-seed required data (Auth roles, Permission roles + permissions).

**Why**: TRUNCATE is fast (no logging), resets sequences, and CASCADE handles foreign keys. Re-seeding is necessary because TRUNCATE removes seed data.

**Alternatives considered**:
- `EnsureCreated()` per class: doesn't run migrations or seed data.
- Transactions that roll back: incompatible with `WebApplicationFactory` scope boundaries.
- Fresh database per class: requires creating/dropping databases, slower.

### D5: Domain events published before SaveChangesAsync

**Decision**: Modify `ApplicationDbContext.SaveChangesAsync` to publish domain events to `IPublishEndpoint` BEFORE `base.SaveChangesAsync`, so MassTransit's outbox interceptor captures them.

**Why**: Currently events are dispatched AFTER `base.SaveChangesAsync`, so the outbox never sees them. This is an architectural gap that makes the outbox tables empty and domain events non-functional as MassTransit messages.

**Alternatives considered**:
- Add `IDomainEventHandler<T>` bridge that publishes to `IPublishEndpoint`: adds boilerplate for every event, doesn't fix the outbox timing.
- Move outbox processing to after dispatch: would require changing MassTransit's outbox behavior, fragile.

### D6: Disable parallel test execution

**Decision**: `[assembly: CollectionBehavior(DisableTestParallelization = true)]` on each test assembly.

**Why**: TRUNCATE is session-scoped and auto-commits. Parallel test classes sharing the same database would conflict. Sequential execution is simpler and fast enough (76 tests × ~200ms ≈ 15s).

**Alternatives considered**:
- Database-per-class: complex, slow (creating/dropping databases).
- Row-level isolation with GUIDs: impractical with TRUNCATE.
- xUnit `[Collection]` with shared fixture: doesn't prevent parallel execution within a collection.

## Risks / Trade-offs

- **Testcontainers Docker dependency** → CI must have Docker. GitHub-hosted runners have it. Self-hosted runners need Docker installed.
- **Static container across assemblies** → if one assembly's tests crash, the container may be left in a dirty state. Mitigation: Resource Reaper (Ryuk) auto-cleans.
- **TRUNCATE deletes seed data** → re-seeding must happen after every TRUNCATE. Missing a table causes test failures. Mitigation: centralize seed logic in test helpers.
- **Permission API JWT validation fails silently** → `IssuerSigningKeyResolver` throws when JWKS URL is unreachable, but `/permissions/resolve` is `AllowAnonymous` so requests continue. Mitigation: test with no JWT token on Permission API requests.
- **SaveChangesAsync modification** → changes the dispatch order for all callers, not just tests. Mitigation: the new order is strictly better (events are published AND dispatched).
- **WebApplicationFactory in Expense tests** → two factories running simultaneously increases memory usage. Mitigation: factories are lazy and lightweight (in-memory TestServer).
