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

# Generate initial SQL scripts for all databases (from existing migrations)
generate-sql-init:
    mkdir -p docker/migrations/{auth-db,permission-db,expense-db}
    dotnet ef migrations script \
      --project {{auth_migrations}} --startup-project {{auth_startup}} --context {{auth_context}} \
      --idempotent --output docker/migrations/auth-db/001-initial.sql
    dotnet ef migrations script \
      --project {{perm_migrations}} --startup-project {{perm_startup}} --context {{perm_context}} \
      --idempotent --output docker/migrations/permission-db/001-initial.sql
    dotnet ef migrations script \
      --project {{expense_migrations}} --startup-project {{expense_startup}} --context {{expense_context}} \
      --idempotent --output docker/migrations/expense-db/001-initial.sql

# Clean all build artifacts
clean:
    dotnet clean {{solution}}
