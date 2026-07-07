# AGENTS.md

.NET 10 ExpenseTracker (DDD + CQRS). 5 projects, PostgreSQL, JWT, Serilog.

## Application architecture

```
src/
├── SharedKernel/     — DDD abstractions (Entity, Error, Result, IDomainEvent, IDateTimeProvider)
├── Domain/           — entities, value objects, domain events (Expense, Category, User) — depends only on SharedKernel
├── Application/      — CQRS commands/queries, handlers, validation — depends on Domain + SharedKernel
├── Infrastructure/   — EF Core (Npgsql), JWT auth, permissions, Serilog — depends on Application
└── Web.Api/          — minimal API endpoints, middleware, Program (entrypoint) — depends on Infrastructure
tests/
└── ArchitectureTests/ — NetArchTest layer dependency enforcement
```

- **Dependency flow**: SharedKernel → Domain → Application → Infrastructure → Web.Api (innermost→outer)
- **No Controllers** — endpoints are `IEndpoint` implementations registered via Scrutor scanning
- **CQRS**: `ICommand<TResponse>`, `IQuery<TResponse>`, handlers decorated with logging + validation via Scrutor's `Decorate`
- **DI registration**: Scrutor scans assemblies in `Application.DependencyInjection`; `Infrastructure.DependencyInjection` wires EF, auth
- **Auth**: JWT bearer + custom permission-based `IAuthorizationHandler` and `PermissionAuthorizationPolicyProvider`
- **Domain events**: dispatched via `IDomainEventsDispatcher` before `SaveChangesAsync`
- **Result pattern**: `Result<T>` from SharedKernel, mapped to HTTP via `CustomResults.Problem`
- **Explicit typing** preferred (`.editorconfig`: `csharp_style_var_elsewhere = false`)
- **File-scoped namespaces**, `is null`/`is not null` checks, `internal sealed` types by default

## Running

```bash
# Alias for brevity (substitute in commands below)
DC="docker compose -f .devcontainer/docker-compose.yml exec -u developer -w /workspace app"

# Start application services
docker compose -f .devcontainer/docker-compose.yml up -d

# Open a shell in the app container
$DC sh

# Build, test, run
$DC dotnet build ExpenseTracker.slnx
$DC dotnet test tests/ArchitectureTests/ArchitectureTests.csproj
$DC dotnet run --project src/Web.Api/Web.Api.csproj

# EF migrations
$DC dotnet ef migrations add <name> --project src/Infrastructure --startup-project src/Web.Api
$DC dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api

# Use just recipes (run inside the app container)
$DC just build
$DC just api
$DC just test
$DC just test-all
$DC just format
$DC just watch
$DC just clean
$DC just add-migration "MigrationName"
$DC just migrate
$DC just rollback
$DC just rollback-to "MigrationName"
$DC just remove-migration
$DC just list-migrations
```

CI runs `dotnet restore → build → test → publish ExpenseTracker.slnx` on push to `main` (.NET 10.x).

## Infrastructure services

| Service | Port | Notes |
|---------|------|-------|
| web-api | 5000 (HTTP), 5001 (HTTPS) | .NET app, JWT auth |
| postgres | 5432 | Database: `expense-tracker`, user/pass: `postgres` |
| seq | 8081 | Structured log viewer |

Connection strings reference `host.docker.internal` for reaching the host from containers.

## Development rules

- **ALL development MUST happen inside the app container.** Never install SDKs, tools, or databases on the host.
- Start infra services (`postgres`, `seq`) with `docker compose -f .devcontainer/docker-compose.yml up -d`.
- Run dotnet commands via `$DC <command>` or open a shell with `$DC sh`.
- The Docker environment is defined in `.devcontainer/` and is the single source of truth.
- `docker compose` commands use Docker socket shared from the host (DooD) — no Docker-in-Docker.

## Key differences from defaults

- `TreatWarningsAsErrors` + `AnalysisMode=All` + `EnforceCodeStyleInBuild` enabled (`Directory.Build.props`)
- SonarAnalyzer.CSharp as analyzer in all non-dcproj projects
- Central package management (`Directory.Packages.props`)
- `justfile` with shortcuts for common tasks (`build`, `api`, `test`, migrations) — run via `$DC just <recipe>` or inside a container shell
- `.editorconfig` is 419 lines with strict CA/IDE/Sonar rules — many disabled intentionally
