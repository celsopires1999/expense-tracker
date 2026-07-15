# Service ports
#   Auth.Api       → 5100  (Swagger: http://localhost:5100/swagger)
#   Permission.Api → 5200  (Swagger: http://localhost:5200/swagger)
#   Expense.Api    → 5000  (Swagger: http://localhost:5000/swagger)
#
# Databases: auth-db, permission-db, expense-db (PostgreSQL :5432)
# Log viewer: Seq → http://localhost:8082

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
    dotnet ef migrations add {{name}} --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

migrate-auth:
    dotnet ef database update --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

rollback-auth:
    dotnet ef database update 0 --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

rollback-to-auth name:
    dotnet ef database update {{name}} --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

remove-migration-auth:
    dotnet ef migrations remove --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

list-migrations-auth:
    dotnet ef migrations list --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}

# ──────────── Migrations — Permission ────────────

add-migration-perm name:
    dotnet ef migrations add {{name}} --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

migrate-perm:
    dotnet ef database update --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

rollback-perm:
    dotnet ef database update 0 --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

rollback-to-perm name:
    dotnet ef database update {{name}} --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

remove-migration-perm:
    dotnet ef migrations remove --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

list-migrations-perm:
    dotnet ef migrations list --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}

# ──────────── Migrations — Expense ────────────

add-migration-expense name:
    dotnet ef migrations add {{name}} --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

migrate-expense:
    dotnet ef database update --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

rollback-expense:
    dotnet ef database update 0 --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

rollback-to-expense name:
    dotnet ef database update {{name}} --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

remove-migration-expense:
    dotnet ef migrations remove --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

list-migrations-expense:
    dotnet ef migrations list --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

# ──────────── Migrations — All services ────────────

# Add a migration with the same name to all 3 services
add-migration-all name:
    dotnet ef migrations add {{name}} --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}
    dotnet ef migrations add {{name}} --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}
    dotnet ef migrations add {{name}} --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

# Apply pending migrations on all databases
migrate-all: migrate-auth migrate-perm migrate-expense

# Rollback all databases to initial state
rollback-all: rollback-auth rollback-perm rollback-expense

# ──────────── Database reset ────────────

# Rollback all migrations, then re-apply them (clears all data)
reset-db:
    dotnet ef database update 0 --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}
    dotnet ef database update 0 --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}
    dotnet ef database update 0 --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}
    dotnet ef database update --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}}
    dotnet ef database update --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}}
    dotnet ef database update --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}}

# ──────────── Code Quality ────────────

# Format code according to .editorconfig
format:
    dotnet format {{solution}}

# Check formatting without modifying files
format-check:
    dotnet format {{solution}} --verify-no-changes

# ──────────── SQL Migration Scripts ────────────

# Generate SQL scripts mirroring EF migration files (up + down)
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
          dotnet ef migrations script 0 "$migration" --idempotent \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}.sql"
          dotnet ef migrations script "$migration" 0 --idempotent \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}_down.sql"
        else
          dotnet ef migrations script "$prev" "$migration" --idempotent \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}.sql"
          dotnet ef migrations script "$migration" "$prev" --idempotent \
            --project "$project" --startup-project "$startup" --context "$context" \
            --output "docker/migrations/${db_dir}/${migration}_down.sql"
        fi
        echo "Generated: docker/migrations/${db_dir}/${migration}.sql + ${migration}_down.sql"
        prev="$migration"
      done <<< "$migrations"
    done

# Generate incremental SQL for a specific Auth migration
# Usage: just generate-sql-auth <migration-name>
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
      if [ "$line" = "$target" ]; then
        found=true
        break
      fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then
      echo "Error: Migration '$target' not found in {{auth_context}}"
      echo "Available migrations:"
      echo "$migrations"
      exit 1
    fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --idempotent \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --idempotent \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --idempotent \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --idempotent \
        --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
        --output "docker/migrations/auth-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/auth-db/${target}.sql + ${target}_down.sql"

# Generate incremental SQL for a specific Permission migration
# Usage: just generate-sql-perm <migration-name>
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
      if [ "$line" = "$target" ]; then
        found=true
        break
      fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then
      echo "Error: Migration '$target' not found in {{perm_context}}"
      echo "Available migrations:"
      echo "$migrations"
      exit 1
    fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --idempotent \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --idempotent \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --idempotent \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --idempotent \
        --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
        --output "docker/migrations/permission-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/permission-db/${target}.sql + ${target}_down.sql"

# Generate incremental SQL for a specific Expense migration
# Usage: just generate-sql-expense <migration-name>
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
      if [ "$line" = "$target" ]; then
        found=true
        break
      fi
      prev="$line"
    done <<< "$migrations"
    if [ "$found" = false ]; then
      echo "Error: Migration '$target' not found in {{expense_context}}"
      echo "Available migrations:"
      echo "$migrations"
      exit 1
    fi
    if [ -z "$prev" ]; then
      dotnet ef migrations script 0 "$target" --idempotent \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}.sql"
      dotnet ef migrations script "$target" 0 --idempotent \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}_down.sql"
    else
      dotnet ef migrations script "$prev" "$target" --idempotent \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}.sql"
      dotnet ef migrations script "$target" "$prev" --idempotent \
        --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
        --output "docker/migrations/expense-db/${target}_down.sql"
    fi
    echo "Generated: docker/migrations/expense-db/${target}.sql + ${target}_down.sql"

# ──────────── SQL Rollback ────────────

# Rollback a specific Auth migration via SQL
# Usage: just rollback-sql-auth <migration-name>
rollback-sql-auth name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    sql_files=$(ls docker/migrations/auth-db/*_down.sql 2>/dev/null | sort -r)
    if [ -z "$sql_files" ]; then
      echo "No rollback SQL files found in docker/migrations/auth-db/"
      exit 1
    fi
    applied=$(PGPASSWORD=migration_pass psql -U migration_user -d auth-db -h localhost -t -A \
      -c "SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY migration_id;" 2>/dev/null || echo "")
    if ! echo "$applied" | grep -q "^${target}$"; then
      echo "Error: Migration '$target' is not applied to auth-db"
      exit 1
    fi
    to_revert=()
    while IFS= read -r f; do
      [ -z "$f" ] && continue
      mid=$(basename "$f" _down.sql)
      if echo "$applied" | grep -q "^${mid}$"; then
        to_revert+=("$mid")
        if [ "$mid" = "$target" ]; then
          break
        fi
      fi
    done <<< "$sql_files"
    if [ ${#to_revert[@]} -eq 0 ]; then
      echo "Nothing to rollback for '$target' in auth-db"
      exit 0
    fi
    echo "Rolling back auth-db: ${to_revert[*]}"
    for mid in "${to_revert[@]}"; do
      echo "Reverting $mid from auth-db"
      PGPASSWORD=migration_pass psql -U migration_user -d auth-db -h localhost \
        -v ON_ERROR_STOP=1 -f "docker/migrations/auth-db/${mid}_down.sql"
      PGPASSWORD=migration_pass psql -U migration_user -d auth-db -h localhost \
        -c "DELETE FROM public.\"__EFMigrationsHistory\" WHERE migration_id = '${mid}';"
    done
    echo "Rollback complete for auth-db"

# Rollback a specific Permission migration via SQL
# Usage: just rollback-sql-perm <migration-name>
rollback-sql-perm name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    sql_files=$(ls docker/migrations/permission-db/*_down.sql 2>/dev/null | sort -r)
    if [ -z "$sql_files" ]; then
      echo "No rollback SQL files found in docker/migrations/permission-db/"
      exit 1
    fi
    applied=$(PGPASSWORD=migration_pass psql -U migration_user -d permission-db -h localhost -t -A \
      -c "SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY migration_id;" 2>/dev/null || echo "")
    if ! echo "$applied" | grep -q "^${target}$"; then
      echo "Error: Migration '$target' is not applied to permission-db"
      exit 1
    fi
    to_revert=()
    while IFS= read -r f; do
      [ -z "$f" ] && continue
      mid=$(basename "$f" _down.sql)
      if echo "$applied" | grep -q "^${mid}$"; then
        to_revert+=("$mid")
        if [ "$mid" = "$target" ]; then
          break
        fi
      fi
    done <<< "$sql_files"
    if [ ${#to_revert[@]} -eq 0 ]; then
      echo "Nothing to rollback for '$target' in permission-db"
      exit 0
    fi
    echo "Rolling back permission-db: ${to_revert[*]}"
    for mid in "${to_revert[@]}"; do
      echo "Reverting $mid from permission-db"
      PGPASSWORD=migration_pass psql -U migration_user -d permission-db -h localhost \
        -v ON_ERROR_STOP=1 -f "docker/migrations/permission-db/${mid}_down.sql"
      PGPASSWORD=migration_pass psql -U migration_user -d permission-db -h localhost \
        -c "DELETE FROM public.\"__EFMigrationsHistory\" WHERE migration_id = '${mid}';"
    done
    echo "Rollback complete for permission-db"

# Rollback a specific Expense migration via SQL
# Usage: just rollback-sql-expense <migration-name>
rollback-sql-expense name:
    #!/usr/bin/env bash
    set -euo pipefail
    target="{{name}}"
    sql_files=$(ls docker/migrations/expense-db/*_down.sql 2>/dev/null | sort -r)
    if [ -z "$sql_files" ]; then
      echo "No rollback SQL files found in docker/migrations/expense-db/"
      exit 1
    fi
    applied=$(PGPASSWORD=migration_pass psql -U migration_user -d expense-db -h localhost -t -A \
      -c "SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY migration_id;" 2>/dev/null || echo "")
    if ! echo "$applied" | grep -q "^${target}$"; then
      echo "Error: Migration '$target' is not applied to expense-db"
      exit 1
    fi
    to_revert=()
    while IFS= read -r f; do
      [ -z "$f" ] && continue
      mid=$(basename "$f" _down.sql)
      if echo "$applied" | grep -q "^${mid}$"; then
        to_revert+=("$mid")
        if [ "$mid" = "$target" ]; then
          break
        fi
      fi
    done <<< "$sql_files"
    if [ ${#to_revert[@]} -eq 0 ]; then
      echo "Nothing to rollback for '$target' in expense-db"
      exit 0
    fi
    echo "Rolling back expense-db: ${to_revert[*]}"
    for mid in "${to_revert[@]}"; do
      echo "Reverting $mid from expense-db"
      PGPASSWORD=migration_pass psql -U migration_user -d expense-db -h localhost \
        -v ON_ERROR_STOP=1 -f "docker/migrations/expense-db/${mid}_down.sql"
      PGPASSWORD=migration_pass psql -U migration_user -d expense-db -h localhost \
        -c "DELETE FROM public.\"__EFMigrationsHistory\" WHERE migration_id = '${mid}';"
    done
    echo "Rollback complete for expense-db"

# Clean all build artifacts
clean:
    dotnet clean {{solution}}
