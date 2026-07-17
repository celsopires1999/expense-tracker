# Entity Framework Core Logging

EF Core logs (SQL queries, connection lifecycle, query compilation) are **suppressed by default** in development to reduce noise.

## Enable verbose EF logging

In the relevant `appsettings.Development.json`, change the `Microsoft.EntityFrameworkCore` overrides from `Warning` to `Information` (or `Debug`):

```json
"Override": {
  "Microsoft": "Information",
  "Microsoft.EntityFrameworkCore": "Information",
  "Microsoft.EntityFrameworkCore.Database.Command": "Information"
}
```

Set to `Debug` for maximum detail including query parameter values.

## Revert to default

Set the EF overrides back to `Warning`:

```json
"Override": {
  "Microsoft": "Information",
  "Microsoft.EntityFrameworkCore": "Warning",
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
}
```

## Sandbox environment

The sandbox (`docker-compose.sandbox.yml`) configures Serilog entirely via environment variables. Setting `Serilog__MinimumLevel__Override__Microsoft: Warning` suppresses all `Microsoft.*` logs including EF Core.

To enable verbose EF logs in sandbox, change the override to `Information`:

```yaml
Serilog__MinimumLevel__Override__Microsoft: Information
```
