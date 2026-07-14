#!/bin/bash
set -e

for db in auth-db permission-db expense-db; do
    echo "=== Migrations for $db ==="
    for f in /migrations/${db}/*.sql; do
        [ -f "$f" ] || continue
        echo "Applying $(basename "$f") to $db"
        PGPASSWORD=migration_pass psql -U migration_user -d "$db" -h postgres -v ON_ERROR_STOP=1 -f "$f"
    done
done

echo "All migrations applied successfully."
