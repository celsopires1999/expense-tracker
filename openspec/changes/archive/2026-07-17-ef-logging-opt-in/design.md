## Context

All three APIs (Auth, Expense, Permission) share identical Serilog configuration in their `appsettings.Development.json`. The `"Override": { "Microsoft": "Information" }` setting applies `Information` level to all `Microsoft.*` namespaces — including `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Database.Command`. This causes every SQL query, connection open/close, and query compilation event to flood console and Seq sinks during development.

No explicit EF logging configuration exists in code (`DbContext` subclasses, `DependencyInjection.cs`, or `Program.cs`). The noise is purely config-driven.

## Goals / Non-Goals

**Goals:**
- Suppress EF Core logs (SQL commands, connection lifecycle, query compilation) by default in development
- Provide clear, documented opt-in mechanism for developers who need verbose EF logs for debugging
- Apply the change consistently across all three API projects

**Non-Goals:**
- Changing production logging behavior (production `appsettings.json` files have no Serilog section)
- Introducing code-level logging filters or `DiagnosticSource` subscriptions
- Modifying `DbContext` subclasses or `DependencyInjection.cs` files
- Changing the general `Microsoft` override level (other Microsoft namespaces like ASP.NET Core should remain at `Information`)

## Decisions

### Decision 1: Add granular Serilog overrides for EF Core categories

**Choice:** Add `"Microsoft.EntityFrameworkCore": "Warning"` and `"Microsoft.EntityFrameworkCore.Database.Command": "Warning"` to the `Override` section of each `appsettings.Development.json`.

**Rationale:** Serilog's `Override` section uses namespace-prefix matching. By setting `Microsoft.EntityFrameworkCore` to `Warning`, all EF-related logs (query compilation, connection, change tracking) are suppressed unless they reach `Warning` or above. The more specific `Microsoft.EntityFrameworkCore.Database.Command` override ensures SQL command logging is also suppressed independently. This approach:
- Requires zero code changes
- Works with the existing `ReadFrom.Configuration` pattern
- Preserves `Microsoft.AspNetCore` and other non-EF logs at `Information`
- Is reversible by simply commenting/uncommenting lines

**Alternatives considered:**
- *`LogTo()` with a filter in `DbContext`*: Would require code changes in 3 places and mixes concerns (config in code). Rejected.
- *Changing `"Microsoft"` to `"Warning"`*: Would also suppress ASP.NET Core logs, losing useful request/response information. Rejected.
- *Using `Serilog.Filters.Expressions` package*: Overkill for namespace-level filtering. The built-in `Override` section is sufficient.

### Decision 2: Include commented opt-in examples

**Choice:** Add commented-out lines in each `appsettings.Development.json` showing how to set EF categories to `Information` or `Debug`.

**Rationale:** Developers need to know how to enable verbose EF logging without searching documentation. Comments are the simplest, most discoverable mechanism — they appear right next to the suppression config.

## Risks / Trade-offs

- **[Risk] Developer unaware of opt-in** → The commented examples serve as inline documentation. The change is low-stakes — if a developer needs EF logs, they uncomment one line.
- **[Risk] Missed `Warning`-level EF errors** → EF Core rarely logs at `Warning` in normal operation. Critical errors bubble up as exceptions, not log events. Low risk.
- **[Trade-off] No per-developer override** → All developers get the same default. Individual overrides would require user-secrets or env vars, adding complexity for minimal benefit. The commented opt-in is sufficient.
