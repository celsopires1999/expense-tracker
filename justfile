# Service ports
#   Auth.Api       → 5100
#   Permission.Api → 5200
#   Expense.Api    → 5000

auth_migrations := "src/Auth/Auth.Infrastructure"
auth_startup := "src/Auth/Auth.Api"

perm_migrations := "src/Permission/Permission.Infrastructure"
perm_startup := "src/Permission/Permission.Api"

expense_migrations := "src/Expense/Expense.Infrastructure"
expense_startup := "src/Expense/Expense.Api"

db_context := "ApplicationDbContext"

default:
    @just --list

# Build the solution
build:
    dotnet build ExpenseTracker.slnx

# Run Auth.Api (Swagger — http://localhost:5100/swagger)
api-auth: build
    dotnet run --project "{{auth_startup}}"

# Run Permission.Api (Swagger — http://localhost:5200/swagger)
api-perm: build
    dotnet run --project "{{perm_startup}}"

# Run Expense.Api (Swagger — http://localhost:5000/swagger)
api-expense: build
    dotnet run --project "{{expense_startup}}"

# Run all three services (requires 3 terminals)
api: api-auth & api-perm & api-expense

# Auth Service migrations
add-migration-auth name:
    dotnet ef migrations add {{name}} --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

migrate-auth:
    dotnet ef database update --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

rollback-auth:
    dotnet ef database update 0 --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

rollback-to-auth name:
    dotnet ef database update {{name}} --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

remove-migration-auth:
    dotnet ef migrations remove --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

list-migrations-auth:
    dotnet ef migrations list --project "{{auth_migrations}}" --startup-project "{{auth_startup}}" --context {{db_context}}

# Permission Service migrations
add-migration-perm name:
    dotnet ef migrations add {{name}} --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

migrate-perm:
    dotnet ef database update --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

rollback-perm:
    dotnet ef database update 0 --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

rollback-to-perm name:
    dotnet ef database update {{name}} --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

remove-migration-perm:
    dotnet ef migrations remove --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

list-migrations-perm:
    dotnet ef migrations list --project "{{perm_migrations}}" --startup-project "{{perm_startup}}" --context {{db_context}}

# Expense Service migrations
add-migration-expense name:
    dotnet ef migrations add {{name}} --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

migrate-expense:
    dotnet ef database update --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

rollback-expense:
    dotnet ef database update 0 --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

rollback-to-expense name:
    dotnet ef database update {{name}} --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

remove-migration-expense:
    dotnet ef migrations remove --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

list-migrations-expense:
    dotnet ef migrations list --project "{{expense_migrations}}" --startup-project "{{expense_startup}}" --context {{db_context}}

# Run architecture tests
test:
    dotnet test tests/ArchitectureTests

# Run all tests
test-all:
    dotnet test ExpenseTracker.slnx

# Format code according to .editorconfig rules
format:
    dotnet format ExpenseTracker.slnx

# Clean all build artifacts
clean:
    dotnet clean ExpenseTracker.slnx
