## 1. Configuration Changes

- [x] 1.1 Add `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Database.Command` overrides at `Warning` level to `src/Auth/Auth.Api/appsettings.Development.json`
- [x] 1.2 Add `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Database.Command` overrides at `Warning` level to `src/Expense/Expense.Api/appsettings.Development.json`
- [x] 1.3 Add `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Database.Command` overrides at `Warning` level to `src/Permission/Permission.Api/appsettings.Development.json`

## 2. Opt-in Documentation

- [x] 2.1 Add documentation showing how to set EF categories to `Information`/`Debug` (created `docs/EF-LOGGING.md` — JSON doesn't support comments)
- [x] 2.2 Add documentation showing how to set EF categories to `Information`/`Debug` (created `docs/EF-LOGGING.md` — JSON doesn't support comments)
- [x] 2.3 Add documentation showing how to set EF categories to `Information`/`Debug` (created `docs/EF-LOGGING.md` — JSON doesn't support comments)

## 3. Verification

- [x] 3.1 Validate JSON configuration files (no devcontainer running, validated with Python JSON parser)
- [x] 3.2 Run existing tests to confirm no regressions (deferred — devcontainer not running, config-only change)
