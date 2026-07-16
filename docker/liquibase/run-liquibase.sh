#!/bin/bash
set -e

echo "=== Liquibase Migrations ==="

for DB in auth-db permission-db expense-db; do
    echo "--- Applying changesets for $DB ---"
    liquibase \
        --search-path="/liquibase/custom,/liquibase/sql" \
        update \
        --changelog-file="${DB}/changelog.xml" \
        --url="jdbc:postgresql://postgres:5432/${DB}" \
        --username=migration_user \
        --password=migration_pass \
        --log-level=INFO
    echo "--- $DB migrations applied successfully ---"
done

echo "=== All Liquibase migrations completed ==="
