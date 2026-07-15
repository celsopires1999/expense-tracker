#!/bin/bash
set -e

# Usage: run-rollback.sh <db-name> <target-migration>
# Example: run-rollback.sh expense-db 20260710015922_AddOutboxTables
# Reverts all migrations FROM the target onward (including the target itself)

DB="${1:?Usage: run-rollback.sh <db-name> <target-migration>}"
TARGET="${2:?Usage: run-rollback.sh <db-name> <target-migration>}"

echo "=== Rollback $DB to $TARGET ==="

# Check __EFMigrationsHistory exists
table_exists=$(PGPASSWORD=migration_pass psql -U migration_user -d "$DB" -h postgres -t -A \
  -c "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory');" 2>/dev/null)

if [ "$table_exists" != "t" ]; then
    echo "Error: __EFMigrationsHistory not found in $DB. No migrations to rollback."
    exit 1
fi

# Get applied migrations
applied=$(PGPASSWORD=migration_pass psql -U migration_user -d "$DB" -h postgres -t -A \
  -c "SELECT migration_id FROM public.\"__EFMigrationsHistory\" ORDER BY migration_id;" 2>/dev/null)

if ! echo "$applied" | grep -q "^${TARGET}$"; then
    echo "Error: Migration '$TARGET' is not applied to $DB"
    echo "Applied migrations:"
    echo "$applied"
    exit 1
fi

# Find migrations to revert (applied at and after target, in reverse order)
to_revert=()
while IFS= read -r mid; do
    [ -z "$mid" ] && continue
    to_revert+=("$mid")
    if [ "$mid" = "$TARGET" ]; then
        break
    fi
done <<< "$(echo "$applied" | sort -r)"

if [ ${#to_revert[@]} -eq 0 ]; then
    echo "Nothing to rollback for '$TARGET' in $DB"
    exit 0
fi

echo "Migrations to revert: ${to_revert[*]}"

# Apply rollback scripts in reverse order
for mid in "${to_revert[@]}"; do
    rollback_file="/migrations/${DB}/${mid}_down.sql"
    if [ ! -f "$rollback_file" ]; then
        echo "Error: Rollback file not found: $rollback_file"
        exit 1
    fi
    echo "Reverting $mid from $DB"
    PGPASSWORD=migration_pass psql -U migration_user -d "$DB" -h postgres \
      -v ON_ERROR_STOP=1 -f "$rollback_file"
    PGPASSWORD=migration_pass psql -U migration_user -d "$DB" -h postgres \
      -c "DELETE FROM public.\"__EFMigrationsHistory\" WHERE migration_id = '${mid}';"
done

echo "Rollback complete for $DB"
