#!/bin/bash
set -e

echo "Creating databases..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE "auth-db";
    CREATE DATABASE "permission-db";
    CREATE DATABASE "expense-db";
EOSQL

echo "Creating migration_user (DBA)..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER migration_user WITH PASSWORD 'migration_pass';
    ALTER USER migration_user CREATEDB;
EOSQL

for db in auth-db permission-db expense-db; do
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db" <<-EOSQL
        GRANT ALL ON SCHEMA public TO migration_user;
        GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO migration_user;
        GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO migration_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO migration_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO migration_user;
EOSQL
done

echo "Creating application users..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER auth_app WITH PASSWORD 'auth_app_pass';
    CREATE USER perm_app WITH PASSWORD 'perm_app_pass';
    CREATE USER expense_app WITH PASSWORD 'expense_app_pass';
EOSQL

echo "Granting permissions to auth_app on auth-db..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "auth-db" <<-EOSQL
    GRANT CONNECT ON DATABASE "auth-db" TO auth_app;
    GRANT USAGE ON SCHEMA public TO auth_app;
    GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO auth_app;
    GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO auth_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO auth_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO auth_app;
EOSQL

echo "Granting permissions to perm_app on permission-db..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "permission-db" <<-EOSQL
    GRANT CONNECT ON DATABASE "permission-db" TO perm_app;
    GRANT USAGE ON SCHEMA public TO perm_app;
    GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO perm_app;
    GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO perm_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO perm_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO perm_app;
EOSQL

echo "Granting permissions to expense_app on expense-db..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "expense-db" <<-EOSQL
    GRANT CONNECT ON DATABASE "expense-db" TO expense_app;
    GRANT USAGE ON SCHEMA public TO expense_app;
    GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO expense_app;
    GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO expense_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO expense_app;
    ALTER DEFAULT PRIVILEGES FOR USER migration_user IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO expense_app;
EOSQL

echo "Database initialization complete."
