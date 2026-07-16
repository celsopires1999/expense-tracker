# AGENTS.md

.NET 10 ExpenseTracker (DDD + CQRS). 3 bounded contexts, 18 projects, PostgreSQL, JWT, Serilog, MassTransit.

## Application architecture

```
src/
├── SharedKernel/              — DDD abstractions (Entity, Error, Result, IDomainEvent, IDateTimeProvider)
├── Auth/                      — Authentication & User management
│   ├── Auth.Domain/
│   ├── Auth.Application/
│   ├── Auth.Infrastructure/
│   └── Auth.Api/
├── Expense/                   — Expense tracking
│   ├── Expense.Domain/
│   ├── Expense.Application/
│   ├── Expense.Infrastructure/
│   └── Expense.Api/
└── Permission/                — Role & Permission management
    ├── Permission.Domain/
    ├── Permission.Application/
    ├── Permission.Infrastructure/
    └── Permission.Api/
tests/
├── ArchitectureTests/         — NetArchTest layer dependency enforcement
├── Auth.IntegrationTests/     — Auth API integration tests (xunit + Testcontainers)
├── Expense.IntegrationTests/  — Expense API integration tests
├── Permission.IntegrationTests/ — Permission API integration tests
└── TestShared/                — Shared test infrastructure (PostgreSqlFixture, RabbitMQFixture, MigrationApplier)
```

- **Dependency flow**: SharedKernel → Context layers (innermost→outer)
- **No Controllers** — endpoints are `ICommand/IQuery<T>` implementations registered via Scrutor scanning
- **CQRS**: `ICommand<TResponse>`, `IQuery<TResponse>`, handlers decorated with logging + validation via Scrutor's `Decorate`
- **DI registration**: Per-context via Scrutor scanning in `Application.DependencyInjection.cs` and `Infrastructure.DependencyInjection.cs`
- **DbContexts**: Each context has its own — `AuthDbContext`, `PermissionDbContext`, `ApplicationDbContext`
- **Auth**: RSA key-based JWT signing, JWKS endpoint (`/.well-known/jwks`)
- **Expense/Permission**: JWKS-based key resolution from Auth.Api
- **Expense → Permission**: `PermissionServiceClient` HTTP calls from Expense.Api to Permission.Api
- **Permission → Auth**: Role sync via RabbitMQ — Permission publishes `RoleCreated/Updated/DeletedEvent`, Auth consumes to keep local `roles` table in sync
- **Domain events**: dispatched via `IDomainEventsDispatcher` before `SaveChangesAsync`
- **Messaging**: MassTransit with RabbitMQ transport + EF Outbox pattern (per context)
- **Result pattern**: `Result<T>` from SharedKernel, mapped to HTTP via `CustomResults.Problem`
- **Explicit typing** preferred (`.editorconfig`: `csharp_style_var_elsewhere = false`)
- **File-scoped namespaces**, `is null`/`is not null` checks, `internal sealed` types by default

## Running

```bash
# ── Infrastructure services (host) ──
docker compose up -d  # starts development environment + postgres + seq + rabbitmq at root level

# ── Alias for app container commands ──
DC="docker compose exec -u developer -w /workspace app"

# Open a shell in the app container
$DC sh

# ── Build & Test ──
$DC dotnet build ExpenseTracker.slnx
$DC dotnet test tests/ArchitectureTests
$DC dotnet test ExpenseTracker.slnx               # all tests
$DC just test-e2e                                   # E2E script (services must be running)

# ── Run services (inside container) ──
$DC dotnet run --project src/Auth/Auth.Api
$DC dotnet run --project src/Expense/Expense.Api
$DC dotnet run --project src/Permission/Permission.Api

# ── EF migrations (authoring — per context) ──
$DC dotnet ef migrations add <Name> --project src/Auth/Auth.Infrastructure --startup-project src/Auth/Auth.Api --context AuthDbContext
# Same pattern for Permission (PermissionDbContext) and Expense (ApplicationDbContext)

# ── Apply migrations (Liquibase — via justfile) ──
$DC just migrate-auth / migrate-perm / migrate-expense / migrate-all

# ── Justfile recipes (run inside app container) ──
$DC just build
$DC just test
$DC just test-all
$DC just api-auth / api-perm / api-expense / api-all
$DC just watch-auth / watch-perm / watch-expense
$DC just add-migration-auth "Name" / add-migration-perm "Name" / add-migration-expense "Name" / add-migration-all "Name"
$DC just migrate-auth / migrate-perm / migrate-expense / migrate-all
$DC just rollback-auth / rollback-perm / rollback-expense
$DC just rollback-to-auth <migration> / rollback-to-perm <migration> / rollback-to-expense <migration>
$DC just list-migrations-auth / list-migrations-perm / list-migrations-expense
$DC just generate-sql-init                    # generate SQL from EF Core migrations for all contexts
$DC just generate-sql-auth <timestamp>        # generate SQL for a specific auth migration
$DC just generate-sql-perm <timestamp>        # generate SQL for a specific permission migration
$DC just generate-sql-expense <timestamp>     # generate SQL for a specific expense migration
$DC just reset-db
$DC just format / format-check
$DC just clean
```

CI runs `dotnet restore → build → test → publish` individual API projects on push to `main` and `workflow_dispatch` (.NET 10.x). See `.github/workflows/build.yml`.

## Infrastructure services

| Service | Port | Notes |
|---------|------|-------|
| Auth.Api | 5100 | JWT auth, JWKS endpoint (`/.well-known/jwks`) |
| Permission.Api | 5200 | Role & permission management |
| Expense.Api | 5000 | Expense tracking, calls Permission.Api |
| PostgreSQL | 5432 | 3 databases: `auth-db`, `permission-db`, `expense-db` |
| RabbitMQ | 5672 | Message broker for role sync (management UI: `localhost:15672`) |
| Seq | 8082 | Structured log viewer |

Connection strings use `host.docker.internal` for reaching the host from containers.

## Development rules

- **ALL development MUST happen inside the app container.** Never run dotnet commands as root — always use `developer` user in `/workspace`.
- Start infra services at root level with `docker compose up -d` (postgres + seq + rabbitmq).
- Run dotnet commands via `$DC <command>` or open a shell with `$DC sh`.
- The Docker dev environment is defined in `Dockerfile.dev` and is the single source of truth.
- `docker compose` commands use Docker socket shared from the host (DooD) — no Docker-in-Docker.

## Key differences from defaults

- `TreatWarningsAsErrors` + `AnalysisMode=All` + `EnforceCodeStyleInBuild` enabled (`Directory.Build.props`)
- SonarAnalyzer.CSharp as analyzer in all non-dcproj projects
- Central package management (`Directory.Packages.props`)
- `.editorconfig` is 424 lines with strict CA/IDE/Sonar rules — many disabled intentionally
- `justfile` with ~30 recipes for per-context operations — run via `$DC just <recipe>`, or `$DC just --list` to see all available recipes
- `manual-tests/` with `api.http` and `full_test.sh` E2E script
- Liquibase for sandbox/production migration application (EF Core migrations remain the source of truth for authoring)
