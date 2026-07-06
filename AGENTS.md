# AGENTS.md

.NET 10 ExpenseTracker (DDD + CQRS). 5 projects, PostgreSQL, JWT, Serilog.

## Devcontainer

- Workspace mounted at `/workspace` via Docker Compose (DooD pattern — Docker socket shared from host)
- `.devcontainer/Dockerfile.dev`: uses .NET 10 SDK + Docker CLI, buildx, compose plugin, `just`, `dotnet-ef`
- UID/GID/DOCKER_GROUP_ID must match host — set in `.devcontainer/docker-compose.override.yml` (tracked in git)
- Base image is **Ubuntu Noble** — Docker repo URL must use `linux/ubuntu`, not `linux/debian`
- Pre-existing `ubuntu` user at UID 1000 is removed before creating `developer`
- `devcontainer.json`: service `app`, `remoteUser: developer`, workspace at `/workspace`
- After rebuild, `onCreateCommand` chowns workspace + vscode-server volume to `developer`
- Docker port 5000 is bound to **Windows localhost only** — from WSL2 use container networking internally

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
# Start application services (inside devcontainer)
docker compose up -d

# Run single command
dotnet build ExpenseTracker.slnx
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj
dotnet run --project src/Web.Api/Web.Api.csproj

# EF migrations (dotnet-ef pre-installed in devcontainer)
dotnet ef migrations add <name> --project src/Infrastructure --startup-project src/Web.Api
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api
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

- **ALL development MUST happen inside the devcontainer.** Never install SDKs, tools, or databases on the host.
- Start infra services (`postgres`, `seq`) with `docker compose up -d` from inside the devcontainer.
- Code, build, run migrations, and test from the devcontainer's terminal.
- `docker compose` commands run inside the devcontainer via Docker socket (DooD) — no Docker-in-Docker.
- The devcontainer is defined in `.devcontainer/` and is the single source of truth for the dev environment.

## Key differences from defaults

- `TreatWarningsAsErrors` + `AnalysisMode=All` + `EnforceCodeStyleInBuild` enabled (`Directory.Build.props`)
- SonarAnalyzer.CSharp as analyzer in all non-dcproj projects
- Central package management (`Directory.Packages.props`)
- No `justfile` yet (just CLI is installed)
- `.editorconfig` is 419 lines with strict CA/IDE/Sonar rules — many disabled intentionally
