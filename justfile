migrations_project := "src/Infrastructure"
startup_project := "src/Web.Api"
db_context := "ApplicationDbContext"

default:
    @just --list

# Build the solution
build:
    dotnet build ExpenseTracker.slnx

# Run the API (Swagger at /swagger — http://localhost:5000)
api: build
    dotnet run --project "{{startup_project}}"

# Create a new migration (usage: just add-migration "MigrationName")
add-migration name:
    dotnet ef migrations add {{name}} --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Apply all pending migrations
migrate:
    dotnet ef database update --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Revert all migrations (back to empty state)
rollback:
    dotnet ef database update 0 --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Revert to a specific migration (usage: just rollback-to "MigrationName")
rollback-to name:
    dotnet ef database update {{name}} --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Remove the last migration
remove-migration:
    dotnet ef migrations remove --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# List all migrations
list-migrations:
    dotnet ef migrations list --project "{{migrations_project}}" --startup-project "{{startup_project}}" --context {{db_context}}

# Run architecture tests
test:
    dotnet test tests/ArchitectureTests
