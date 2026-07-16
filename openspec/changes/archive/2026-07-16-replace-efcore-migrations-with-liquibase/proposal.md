## Why

The sandbox deployment (`docker-compose.sandbox.yml`) applies database migrations via a hand-written bash script (`run-migrations.sh`) that checks `__EFMigrationsHistory`, iterates SQL files in sorted order, and runs them via `psql`. Rollback is handled by a separate bash script (`run-rollback.sh`) that manually runs `_down.sql` files and deletes history rows. This pipeline is fragile, hard to maintain, and reimplements change tracking that tools like Liquibase provide natively.

Additionally, the `justfile` contains ~150 lines of bash recipes for SQL generation, per-context rollback, and migration history queries — all of which duplicate logic that Liquibase handles out of the box.

EF Core migrations remain the source of truth for schema authoring (`dotnet ef migrations add`). Liquibase replaces only the **apply** mechanism in sandbox/production.

## What Changes

- **Replace bash migration scripts** with Liquibase in `docker-compose.sandbox.yml` — the `migrations` service uses `liquibase/liquibase` image instead of a `postgres:17` image running `run-migrations.sh`.
- **Generate Liquibase-compatible changelogs** from EF Core migration SQL output — each `dotnet ef migrations script` output becomes a Liquibase `<sqlFile>` changeset.
- **Simplify justfile migration recipes** — replace hand-written bash rollback/generate scripts with Liquibase CLI commands (`liquibase update`, `liquibase rollback`).
- **Remove bash migration scripts** — `run-migrations.sh`, `run-rollback.sh`, `000-init.sh` are no longer needed (Liquibase handles tracking and idempotent apply).
- **Remove generated SQL files** from `docker/migrations/` — Liquibase references the EF Core SQL output directly or generates its own.
- **Keep EF Core migration authoring unchanged** — `dotnet ef migrations add`, `dotnet ef migrations script`, C# migration classes all stay. `dotnet-ef` remains in the dev container.
- **Tests unchanged** — integration tests continue using `MigrateAsync()` / `EnsureCreatedAsync()` via EF Core.

## Capabilities

### New Capabilities
- `liquibase-migrations`: Liquibase-based schema migration application for sandbox/production deployment, replacing bash scripts while preserving EF Core as the authoring tool.

### Modified Capabilities

## Impact

- **Docker**: `docker-compose.sandbox.yml` migration service replaced; `docker/migrations/` directory removed.
- **Justfile**: ~10 migration recipes simplified; bash rollback scripts replaced by Liquibase CLI.
- **Dockerfile.dev**: No change — `dotnet-ef` stays.
- **New dependency**: Liquibase Docker image (`liquibase/liquibase`) + PostgreSQL JDBC driver (bundled in image).
- **Database**: `__EFMigrationsHistory` replaced by `DATABASECHANGELOG` + `DATABASECHANGELOGLOCK` in sandbox; tests still use `__EFMigrationsHistory` via EF Core.
- **Tests**: No changes — continue using `MigrateAsync()` / `EnsureCreatedAsync()`.
