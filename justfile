# Service ports
#   Auth.Api       → 5100  (Swagger: http://localhost:5100/swagger)
#   Permission.Api → 5200  (Swagger: http://localhost:5200/swagger)
#   Expense.Api    → 5000  (Swagger: http://localhost:5000/swagger)
#
# Databases: auth-db, permission-db, expense-db (PostgreSQL :5432)
# Log viewer: Seq → http://localhost:8082

# Host workspace path (resolves /workspace → host path for Docker-in-Docker volume mounts)
host_workspace := `docker inspect $(hostname) --format '{{range .Mounts}}{{if eq .Destination "/workspace"}}{{.Source}}{{end}}{{end}}'`

auth_migrations  := "src/Auth/Auth.Infrastructure"
auth_startup     := "src/Auth/Auth.Api"
auth_context     := "AuthDbContext"

perm_migrations  := "src/Permission/Permission.Infrastructure"
perm_startup     := "src/Permission/Permission.Api"
perm_context     := "PermissionDbContext"

expense_migrations := "src/Expense/Expense.Infrastructure"
expense_startup    := "src/Expense/Expense.Api"
expense_context    := "ApplicationDbContext"

solution := "ExpenseTracker.slnx"

default:
    @just --list

# ──────────── Build & Test ────────────

# Build the entire solution
build:
    dotnet build {{solution}}

# Run architecture layer tests
test:
    dotnet test tests/ArchitectureTests

# Run all tests in the solution
test-all:
    dotnet test {{solution}}

# Run the full E2E test suite (requires services running)
test-e2e:
    ./manual-tests/full_test.sh

# ──────────── Run Services ────────────

# Run Auth.Api (port 5100)
api-auth:
    dotnet run --project {{auth_startup}}

# Run Permission.Api (port 5200)
api-perm:
    dotnet run --project {{perm_startup}}

# Run Expense.Api (port 5000)
api-expense:
    dotnet run --project {{expense_startup}}

# Run all services concurrently (background, logs to /tmp/*.log)
api-all:
    dotnet run --project {{auth_startup}} > /tmp/auth.log 2>&1 &
    dotnet run --project {{perm_startup}} > /tmp/perm.log 2>&1 &
    dotnet run --project {{expense_startup}} > /tmp/expense.log 2>&1 &
    echo "Services starting — check /tmp/{auth,perm,expense}.log"

# Stop all three active api services and clean up logs
api-stop:
    @echo "Stopping API services..."
    -pkill -f "dotnet run --project .*({{auth_startup}}|{{perm_startup}}|{{expense_startup}})"
    @echo "Removing log files..."
    rm -f /tmp/auth.log /tmp/perm.log /tmp/expense.log
    @echo "Services stopped and logs cleaned."

# ──────────── Watch (hot reload) ────────────

# Watch Auth.Api (hot reload on port 5100)
watch-auth:
    dotnet watch run --project {{auth_startup}}

# Watch Permission.Api (hot reload on port 5200)
watch-perm:
    dotnet watch run --project {{perm_startup}}

# Watch Expense.Api (hot reload on port 5000)
watch-expense:
    dotnet watch run --project {{expense_startup}}

# ──────────── Migrations — Auth ────────────

add-migration-auth name:
    #!/usr/bin/env bash
    set -euo pipefail
    dotnet ef migrations add {{name}} --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}
    TIMESTAMP=$(dotnet ef migrations list --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | tail -1 | sed 's/ (Pending)$//')
    cat > docker/liquibase/auth-db/changesets/${TIMESTAMP}.xml <<EOF
    <?xml version="1.0" encoding="UTF-8"?>
    <databaseChangeLog
        xmlns="http://www.liquibase.org/xml/ns/dbchangelog"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://www.liquibase.org/xml/ns/dbchangelog
            http://www.liquibase.org/xml/ns/dbchangelog/dbchangelog-latest.xsd">
        <changeSet id="${TIMESTAMP}" author="developer">
            <sqlFile path="auth-db/${TIMESTAMP}.sql" />
            <rollback>
                <sqlFile path="auth-db/${TIMESTAMP}_down.sql" />
            </rollback>
        </changeSet>
    </databaseChangeLog>
    EOF
    sed -i '/<\/databaseChangeLog>/i \    <include file="auth-db/changesets/'${TIMESTAMP}'.xml" />' docker/liquibase/auth-db/changelog.xml
    echo "Created changeset: docker/liquibase/auth-db/changesets/${TIMESTAMP}.xml"
    echo "NOTE: Run 'just generate-sql-auth ${TIMESTAMP}' to generate the SQL file, then re-run this recipe if SQL doesn't exist yet."

migrate-auth:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        update \
        --changelog-file='auth-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/auth-db' \
        --username=migration_user \
        --password=migration_pass"

rollback-auth:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback-count \
        --changelog-file='auth-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/auth-db' \
        --username=migration_user \
        --password=migration_pass \
        --count=1"

rollback-to-auth name:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback \
        --changelog-file='auth-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/auth-db' \
        --username=migration_user \
        --password=migration_pass \
        --tag={{name}}"

list-migrations-auth:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        status \
        --changelog-file='auth-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/auth-db' \
        --username=migration_user \
        --password=migration_pass"

# ──────────── Migrations — Permission ────────────

add-migration-perm name:
    #!/usr/bin/env bash
    set -euo pipefail
    dotnet ef migrations add {{name}} --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}
    TIMESTAMP=$(dotnet ef migrations list --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | tail -1 | sed 's/ (Pending)$//')
    cat > docker/liquibase/permission-db/changesets/${TIMESTAMP}.xml <<EOF
    <?xml version="1.0" encoding="UTF-8"?>
    <databaseChangeLog
        xmlns="http://www.liquibase.org/xml/ns/dbchangelog"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://www.liquibase.org/xml/ns/dbchangelog
            http://www.liquibase.org/xml/ns/dbchangelog/dbchangelog-latest.xsd">
        <changeSet id="${TIMESTAMP}" author="developer">
            <sqlFile path="permission-db/${TIMESTAMP}.sql" />
            <rollback>
                <sqlFile path="permission-db/${TIMESTAMP}_down.sql" />
            </rollback>
        </changeSet>
    </databaseChangeLog>
    EOF
    sed -i '/<\/databaseChangeLog>/i \    <include file="permission-db/changesets/'${TIMESTAMP}'.xml" />' docker/liquibase/permission-db/changelog.xml
    echo "Created changeset: docker/liquibase/permission-db/changesets/${TIMESTAMP}.xml"
    echo "NOTE: Run 'just generate-sql-perm ${TIMESTAMP}' to generate the SQL file, then re-run this recipe if SQL doesn't exist yet."

migrate-perm:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        update \
        --changelog-file='permission-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/permission-db' \
        --username=migration_user \
        --password=migration_pass"

rollback-perm:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback-count \
        --changelog-file='permission-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/permission-db' \
        --username=migration_user \
        --password=migration_pass \
        --count=1"

rollback-to-perm name:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback \
        --changelog-file='permission-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/permission-db' \
        --username=migration_user \
        --password=migration_pass \
        --tag={{name}}"

list-migrations-perm:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        status \
        --changelog-file='permission-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/permission-db' \
        --username=migration_user \
        --password=migration_pass"

# ──────────── Migrations — Expense ────────────

add-migration-expense name:
    #!/usr/bin/env bash
    set -euo pipefail
    dotnet ef migrations add {{name}} --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}
    TIMESTAMP=$(dotnet ef migrations list --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | tail -1 | sed 's/ (Pending)$//')
    cat > docker/liquibase/expense-db/changesets/${TIMESTAMP}.xml <<EOF
    <?xml version="1.0" encoding="UTF-8"?>
    <databaseChangeLog
        xmlns="http://www.liquibase.org/xml/ns/dbchangelog"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://www.liquibase.org/xml/ns/dbchangelog
            http://www.liquibase.org/xml/ns/dbchangelog/dbchangelog-latest.xsd">
        <changeSet id="${TIMESTAMP}" author="developer">
            <sqlFile path="expense-db/${TIMESTAMP}.sql" />
            <rollback>
                <sqlFile path="expense-db/${TIMESTAMP}_down.sql" />
            </rollback>
        </changeSet>
    </databaseChangeLog>
    EOF
    sed -i '/<\/databaseChangeLog>/i \    <include file="expense-db/changesets/'${TIMESTAMP}'.xml" />' docker/liquibase/expense-db/changelog.xml
    echo "Created changeset: docker/liquibase/expense-db/changesets/${TIMESTAMP}.xml"
    echo "NOTE: Run 'just generate-sql-expense ${TIMESTAMP}' to generate the SQL file, then re-run this recipe if SQL doesn't exist yet."

migrate-expense:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        update \
        --changelog-file='expense-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/expense-db' \
        --username=migration_user \
        --password=migration_pass"

rollback-expense:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback-count \
        --changelog-file='expense-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/expense-db' \
        --username=migration_user \
        --password=migration_pass \
        --count=1"

rollback-to-expense name:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        rollback \
        --changelog-file='expense-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/expense-db' \
        --username=migration_user \
        --password=migration_pass \
        --tag={{name}}"

list-migrations-expense:
    docker run --rm --network host \
      -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
      -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
      expense-tracker-migrations bash -c "liquibase \
        --search-path='/liquibase/custom,/liquibase/sql' \
        status \
        --changelog-file='expense-db/changelog.xml' \
        --url='jdbc:postgresql://localhost:5432/expense-db' \
        --username=migration_user \
        --password=migration_pass"

# ──────────── Migrations — All services ────────────

# Add a migration with the same name to all 3 services
add-migration-all name:
    just add-migration-auth {{name}}
    just add-migration-perm {{name}}
    just add-migration-expense {{name}}

# Apply pending migrations on all databases
migrate-all: migrate-auth migrate-perm migrate-expense

# Rollback all databases
rollback-all: rollback-auth rollback-perm rollback-expense

# ──────────── Database initialization ────────────

# Create databases, migration_user and grant permissions (development)
init-db:
    #!/usr/bin/env bash
    set -euo pipefail
    PG="docker exec postgres psql -U postgres"
    echo "Creating databases..."
    for db in auth-db permission-db expense-db; do
      $PG -tc "SELECT 1 FROM pg_database WHERE datname = '${db}'" | grep -q 1 || \
        $PG -c "CREATE DATABASE \"${db}\";"
    done
    echo "Creating migration_user..."
    $PG -tc "SELECT 1 FROM pg_roles WHERE rolname = 'migration_user'" | grep -q 1 || \
      $PG -c "CREATE USER migration_user WITH PASSWORD 'migration_pass'; ALTER USER migration_user CREATEDB;"
    echo "Granting permissions to migration_user..."
    for db in auth-db permission-db expense-db; do
      $PG -d "$db" -c \
        "GRANT ALL ON SCHEMA public TO migration_user;
         GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO migration_user;
         GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO migration_user;
         ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO migration_user;
         ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO migration_user;"
    done
    echo "Database initialization complete."

# ──────────── Database reset ────────────

# Drop all tables and re-apply all migrations (clears all data)
reset-db:
    #!/usr/bin/env bash
    set -euo pipefail
    for db in auth-db permission-db expense-db; do
      echo "Dropping and re-applying $db..."
      docker run --rm --network host \
        -v "{{host_workspace}}/docker/liquibase:/liquibase/custom:ro" \
        -v "{{host_workspace}}/docker/migrations:/liquibase/sql:ro" \
        expense-tracker-migrations bash -c "liquibase \
          drop-all \
          --url='jdbc:postgresql://localhost:5432/${db}' \
          --username=migration_user \
          --password=migration_pass && \
        liquibase \
          --search-path='/liquibase/custom,/liquibase/sql' \
          update \
          --changelog-file='${db}/changelog.xml' \
          --url='jdbc:postgresql://localhost:5432/${db}' \
          --username=migration_user \
          --password=migration_pass"
    done

# ──────────── Code Quality ────────────

# Format code according to .editorconfig
format:
    dotnet format {{solution}}

# Check formatting without modifying files
format-check:
    dotnet format {{solution}} --verify-no-changes

# ──────────── SQL Generation (for Liquibase changesets) ────────────

# Generate SQL scripts for all contexts (run after add-migration-*)
generate-sql-init:
    #!/usr/bin/env bash
    set -euo pipefail
    mkdir -p docker/migrations/{auth-db,permission-db,expense-db}
    contexts=(
      "{{auth_migrations}}|{{auth_startup}}|{{auth_context}}|auth-db"
      "{{perm_migrations}}|{{perm_startup}}|{{perm_context}}|permission-db"
      "{{expense_migrations}}|{{expense_startup}}|{{expense_context}}|expense-db"
    )
    for ctx in "${contexts[@]}"; do
      IFS='|' read -r project startup context db_dir <<< "$ctx"
      prev=""
      migrations=$(dotnet ef migrations list \
        --project "$project" --startup-project "$startup" --context "$context" 2>/dev/null \
        | grep -E '^[0-9]{14}_' | sed 's/ (Pending)$//')
      while IFS= read -r migration; do
        [ -z "$migration" ] && continue
        if [ -z "$prev" ]; then
          dotnet ef migrations script 0 "$migration" --no-transactions \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}.sql"
          dotnet ef migrations script "$migration" 0 --no-transactions \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}_down.sql"
        else
          dotnet ef migrations script "$prev" "$migration" --no-transactions \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}.sql"
          dotnet ef migrations script "$migration" "$prev" --no-transactions \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}_down.sql"
        fi
        echo "Generated: docker/migrations/${db_dir}/${migration}.sql + ${migration}_down.sql"
        prev="$migration"
      done <<< "$migrations"
    done

# Generate SQL for a specific Auth migration
generate-sql-auth name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    migrations=$(dotnet ef migrations list \
      --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | sed 's/ (Pending)$//')
    prev=""
    found=false
    while IFS= read -r line; do
      [ -z "$line" ] && continue
      if [ "$line" = "$target" ]; then found=true; break; fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then echo "Error: Migration '$target' not found"; exit 1; fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --no-transactions \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --no-transactions \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --no-transactions \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --no-transactions \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/auth-db/${target}.sql + ${target}_down.sql"

# Generate SQL for a specific Permission migration
generate-sql-perm name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    migrations=$(dotnet ef migrations list \
      --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | sed 's/ (Pending)$//')
    prev=""
    found=false
    while IFS= read -r line; do
      [ -z "$line" ] && continue
      if [ "$line" = "$target" ]; then found=true; break; fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then echo "Error: Migration '$target' not found"; exit 1; fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --no-transactions \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --no-transactions \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --no-transactions \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --no-transactions \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/permission-db/${target}.sql + ${target}_down.sql"

# Generate SQL for a specific Expense migration
generate-sql-expense name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    migrations=$(dotnet ef migrations list \
      --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} 2>/dev/null \
      | grep -E '^[0-9]{14}_' | sed 's/ (Pending)$//')
    prev=""
    found=false
    while IFS= read -r line; do
      [ -z "$line" ] && continue
      if [ "$line" = "$target" ]; then found=true; break; fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then echo "Error: Migration '$target' not found"; exit 1; fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --no-transactions \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --no-transactions \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --no-transactions \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --no-transactions \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/expense-db/${target}.sql + ${target}_down.sql"

# Clean all build artifacts
clean:
    dotnet clean {{solution}}
