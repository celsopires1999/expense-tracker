#!/bin/bash
set -e

for db in auth-db permission-db expense-db; do
    echo "=== Migrations for $db ==="

    table_exists=$(PGPASSWORD=migration_pass psql -U migration_user -d "$db" -h postgres -t -A \
      -c "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory');" 2>/dev/null)

    if [ "$table_exists" = "t" ]; then
        applied=$(PGPASSWORD=migration_pass psql -U migration_user -d "$db" -h postgres -t -A \
          -c "SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY migration_id;" 2>/dev/null)
    else
        applied=""
    fi

    sql_files=$(ls /migrations/${db}/*.sql 2>/dev/null | grep -v '_down.sql$' | sort)

    if [ -z "$sql_files" ]; then
        echo "No migration files found for $db"
        continue
    fi

    for f in $sql_files; do
        migration_name=$(basename "$f" .sql)

        if echo "$applied" | grep -q "^${migration_name}$"; then
            echo "SKIP (already applied): $migration_name"
            continue
        fi

        echo "Applying $migration_name to $db"
        PGPASSWORD=migration_pass psql -U migration_user -d "$db" -h postgres \
          -v ON_ERROR_STOP=1 -f "$f"
    done
done

echo "All migrations applied successfully."
