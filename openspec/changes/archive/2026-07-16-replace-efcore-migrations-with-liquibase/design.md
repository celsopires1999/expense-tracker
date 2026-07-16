## Context

The expense-tracker uses 3 PostgreSQL databases (auth-db, permission-db, expense-db). The current sandbox migration pipeline is:

1. `dotnet ef migrations add` → C# migration classes (source of truth)
2. `dotnet ef migrations script` → SQL files in `docker/migrations/{db}/*.sql`
3. `run-migrations.sh` → checks `__EFMigrationsHistory`, runs unapplied SQL files via `psql`
4. `run-rollback.sh` → runs `_down.sql` files, deletes history rows

This requires hand-maintained bash scripts that re-implement change tracking. Liquibase provides this natively.

**Tests are NOT affected** — they use `MigrateAsync()` / `EnsureCreatedAsync()` directly via EF Core, bypassing the bash pipeline entirely.

## Goals / Non-Goals

**Goals:**
- Replace bash migration scripts with Liquibase for sandbox/production apply
- Keep EF Core as the schema authoring tool (`dotnet ef migrations add` stays)
- Simplify justfile rollback/generate recipes
- Maintain the same 3-database isolation

**Non-Goals:**
- Changing how migrations are authored (EF Core stays)
- Modifying integration tests (they continue using EF Core directly)
- Removing `dotnet-ef` from the dev container
- Changing the application ORM or DbContext configuration

## Decisions

### 1. EF Core SQL as Liquibase input

**Decision**: Use `dotnet ef migrations script` output as Liquibase `<sqlFile>` changesets. Each EF Core migration becomes one Liquibase changeset that points to the generated SQL file.

**Flow**:
```
dotnet ef migrations add "Name"           # author C# migration
dotnet ef migrations script               # generate SQL
# SQL files become Liquibase changesets
liquibase update                          # apply via Liquibase
```

**Alternatives considered**:
- **Rewrite migrations as native Liquibase XML**: More declarative but requires learning Liquibase XML syntax and maintaining a second representation. Rejected per user preference.
- **Use `liquibase diff-changeLog` against EF Core-managed DB**: Generates changelog from actual schema diff. More automated but harder to review and audit. Could be a future enhancement.

**Rationale**: Minimal learning curve — developers continue using `dotnet ef` as before. Liquibase only handles application, which is the part that needs improvement.

### 2. Changelog structure

**Decision**: Each database gets a master `changelog.xml` that includes per-migration changesets referencing the EF-generated SQL.

```
docker/liquibase/
├── auth-db/
│   ├── changelog.xml
│   └── changesets/
│       ├── 20260710013745_InitialCreate.xml       (sqlFile → ../../sql/auth-db/20260710013745_InitialCreate.sql)
│       └── 20260710015920_AddOutboxTables.xml
├── permission-db/
│   └── ...
└── expense-db/
    └── ...
```

**Rationale**: Separates changelogs (Liquibase metadata) from SQL files (EF Core output). The changelog XML files are thin wrappers — the SQL remains the authoritative content.

### 3. Liquibase container in sandbox

**Decision**: Replace the `migrations` service in `docker-compose.sandbox.yml` with a Liquibase container that runs `liquibase update` for each database.

**Implementation**: A small entrypoint script that runs Liquibase CLI three times (once per database), using environment variables for connection details.

**Alternatives considered**:
- **Single Liquibase invocation with multiple contexts**: Liquibase supports `--contexts=auth,perm,expense` but requires all changelogs in one directory. Adds complexity for marginal benefit.
- **Liquibase in the app container**: Avoids extra container but couples migration tooling to the runtime. The ephemeral-container pattern is cleaner.

**Rationale**: Matches the current pattern (ephemeral container that completes before APIs start). Simple entrypoint script.

### 4. Justfile recipes

**Decision**: Replace bash-heavy recipes with Liquibase CLI wrappers.

| Current | New |
|---------|-----|
| `add-migration-auth "Name"` | `dotnet ef migrations add` (unchanged) |
| `migrate-auth` | `docker run liquibase update --changelog-dir=... --url=jdbc:postgresql://...` |
| `rollback-auth` | `docker run liquibase rollback-count 1` |
| `rollback-to-auth <name>` | `docker run liquibase rollback <tag>` |
| `list-migrations-auth` | `docker run liquibase status` |
| `generate-sql-*` | Removed (Liquibase applies SQL directly) |
| `rollback-sql-*` | Removed (Liquibase handles rollback natively) |
| `reset-db` | `docker run liquibase drop-all` + `liquibase update` |

**Rationale**: Liquibase CLI provides all these operations natively. The justfile becomes thinner.

### 5. Database user for Liquibase

**Decision**: Liquibase connects as `migration_user` (same user currently used by `run-migrations.sh`). This user needs DDL privileges.

**Rationale**: The `000-init.sh` script already creates `migration_user` with `CREATEDB` and full schema grants. Liquibase uses the same permissions.

## Risks / Trade-offs

- **[Risk] Liquibase tracking table mismatch** → On first Liquibase run against an existing database, the `DATABASECHANGELOG` table is empty but the schema already exists. Solution: Use `liquibase clearCheckSums` and a baseline changeset that marks the current state as applied.
- **[Risk] SQL file path coupling** → Liquibase changelog XML references SQL files by relative path. If the `docker/migrations/` directory structure changes, changelogs break. Mitigation: Fixed directory convention, validated in CI.
- **[Trade-off] Two tools instead of one** → Developers need both `dotnet ef` (authoring) and Liquibase (apply). Mitigation: justfile recipes abstract the Liquibase commands; developers rarely run Liquibase directly.
- **[Trade-off] No native down migrations in Liquibase from EF Core** → The `_down.sql` files generated by EF Core are not natively usable by Liquibase rollback. Liquibase needs explicit rollback changesets or `rollbackSQL` attributes. Mitigation: For sandbox, `drop-all` + re-apply is sufficient. For production, rollback changesets can be authored manually.
