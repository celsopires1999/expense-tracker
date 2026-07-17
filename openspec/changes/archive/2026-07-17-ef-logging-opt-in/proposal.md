## Why

EF Core logs are implicitly visible in development because Serilog's `"Override": { "Microsoft": "Information" }` applies to all `Microsoft.*` namespaces, including `Microsoft.EntityFrameworkCore`. This produces noisy SQL query logs, connection lifecycle events, and query compilation details on every request — drowning out meaningful application logs. Developers should only see EF logs when they explicitly opt in for debugging.

## What Changes

- Add a Serilog override for `Microsoft.EntityFrameworkCore` at `Warning` level in all three `appsettings.Development.json` files (Auth, Expense, Permission), suppressing verbose EF logs by default
- Add a commented-out example in `appsettings.Development.json` showing how to enable detailed EF logging (set `Microsoft.EntityFrameworkCore` to `Information` or `Debug`)
- Add `Microsoft.EntityFrameworkCore.Database.Command` at `Warning` to specifically suppress SQL command logging

## Capabilities

### New Capabilities
- `ef-logging-configuration`: Controls Entity Framework Core log verbosity via Serilog configuration, defaulting to suppressed and opt-in per developer

### Modified Capabilities

## Impact

- **Files**: `src/Auth/Auth.Api/appsettings.Development.json`, `src/Expense/Expense.Api/appsettings.Development.json`, `src/Permission/Permission.Api/appsettings.Development.json`
- **Behavior**: EF Core SQL queries, connection open/close, and query compilation logs will no longer appear in development console or Seq by default
- **No breaking changes**: production `appsettings.json` files have no Serilog section and are unaffected
- **No code changes**: purely configuration-level
