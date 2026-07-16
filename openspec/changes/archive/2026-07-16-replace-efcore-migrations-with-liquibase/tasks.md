## 1. Create Liquibase changelog structure

- [x] 1.1 Create `docker/liquibase/auth-db/changesets/`, `docker/liquibase/permission-db/changesets/`, `docker/liquibase/expense-db/changesets/` directories
- [x] 1.2 Create master `changelog.xml` for each database that includes all changeset files in order

## 2. Convert existing EF Core SQL to Liquibase changesets

- [x] 2.1 Create `001-initial-create.xml` for auth-db — `<sqlFile>` pointing to `../../migrations/auth-db/20260710013745_InitialCreate.sql`
- [x] 2.2 Create `002-add-outbox-tables.xml` for auth-db — `<sqlFile>` pointing to `../../migrations/auth-db/20260710015920_AddOutboxTables.sql`
- [x] 2.3 Create `001-initial-create.xml` for permission-db — `<sqlFile>` pointing to `../../migrations/permission-db/20260710013800_InitialCreate.sql`
- [x] 2.4 Create `002-add-outbox-tables.xml` for permission-db — `<sqlFile>` pointing to `../../migrations/permission-db/20260710015920_AddOutboxTables.sql`
- [x] 2.5 Create `001-initial-create.xml` for expense-db — `<sqlFile>` pointing to `../../migrations/expense-db/20260710014742_InitialCreate.sql`
- [x] 2.6 Create `002-add-outbox-tables.xml` for expense-db — `<sqlFile>` pointing to `../../migrations/expense-db/20260710015922_AddOutboxTables.sql`
- [x] 2.7 Create `003-add-expense-status.xml` for expense-db — `<sqlFile>` pointing to `../../migrations/expense-db/20260714212316_AddExpenseStatus.sql`

## 3. Update sandbox Docker Compose

- [x] 3.1 Replace the `migrations` service in `docker-compose.sandbox.yml` with Liquibase container using `liquibase/liquibase:latest`
- [x] 3.2 Create entrypoint script that runs `liquibase update` for each of the 3 databases
- [x] 3.3 Mount `docker/liquibase/` and `docker/migrations/` as volumes in the container
- [x] 3.4 Configure Liquibase connection via environment variables (host, port, username, password per database)
- [x] 3.5 Remove volume mounts for old bash scripts (`run-migrations.sh`, `run-rollback.sh`)

## 4. Update justfile recipes

- [x] 4.1 Update `migrate-auth` / `migrate-perm` / `migrate-expense` to use `docker run liquibase update` with correct changelog path and JDBC URL
- [x] 4.2 Update `rollback-auth` / `rollback-perm` / `rollback-expense` to use `docker run liquibase rollback-count 1`
- [x] 4.3 Update `rollback-to-auth` / `rollback-to-perm` / `rollback-to-expense` to use `docker run liquibase rollback <tag>`
- [x] 4.4 Update `list-migrations-auth` / `list-migrations-perm` / `list-migrations-expense` to use `docker run liquibase status`
- [x] 4.5 Update `add-migration-auth` / `add-migration-perm` / `add-migration-expense` to also scaffold a Liquibase changeset XML file after creating the EF Core migration
- [x] 4.6 Update `add-migration-all` to scaffold changesets for all 3 databases
- [x] 4.7 Update `migrate-all` and `rollback-all` to use Liquibase commands
- [x] 4.8 Update `reset-db` to use `liquibase drop-all` + `liquibase update` per database
- [x] 4.9 Remove `generate-sql-*` recipes (Liquibase applies SQL directly; `dotnet ef migrations script` is still available ad-hoc)
- [x] 4.10 Remove `rollback-sql-*` recipes (Liquibase handles rollback natively)
- [x] 4.11 Remove `remove-migration-*` recipes (Liquibase doesn't support this; manage changeset files manually)

## 5. Remove old bash scripts

- [x] 5.1 Delete `docker/migrations/run-migrations.sh`
- [x] 5.2 Delete `docker/migrations/run-rollback.sh`
- [x] 5.3 Delete `docker/migrations/000-init.sh` (database creation handled by `docker/migrations/` init script or Liquibase)

## 6. Verify schema parity

- [x] 6.1 Start a fresh PostgreSQL instance via `docker compose -f docker-compose.sandbox.yml up postgres`
- [x] 6.2 Run `docker compose -f docker-compose.sandbox.yml up migrations` and verify Liquibase applies all changesets successfully
- [x] 6.3 Compare resulting schema against expected schema (tables, columns, indexes, constraints, seed data)
- [x] 6.4 Verify `DATABASECHANGELOG` table contains entries for all applied changesets
- [x] 6.5 Run `dotnet build` to ensure application code still compiles
- [x] 6.6 Run `dotnet test tests/ArchitectureTests` to verify layer dependencies
