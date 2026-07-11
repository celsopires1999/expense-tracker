## Why

The solution has zero integration or unit tests — only architecture layer dependency tests exist. Business logic in handlers, endpoints, and cross-service communication is unverified. This makes refactoring risky and regressions invisible. We need automated integration tests that exercise the real code paths against real PostgreSQL databases using Testcontainers, covering all three bounded contexts (Auth, Expense, Permission).

## What Changes

- Add `TestShared` project with shared PostgreSQL container fixture (static lazy singleton) and EF migration applier
- Add `Auth.IntegrationTests` project: handler-level (Level 2) and endpoint-level (Level 3) tests for user registration, login, JWT issuance, role assignment, and JWKS discovery
- Add `Expense.IntegrationTests` project: handler and endpoint tests for Categories, Expenses, PaymentMethods, Tags — including a real Permission service via `WebApplicationFactory` (in-process, no Docker)
- Add `Permission.IntegrationTests` project: handler and endpoint tests for role CRUD, permission resolution, and role-permission mapping
- Fix `ApplicationDbContext.SaveChangesAsync` to publish domain events to `IPublishEndpoint` **before** `base.SaveChangesAsync` so MassTransit outbox captures them
- Add domain event outbox verification tests in Expense
- Add `Testcontainers`, `Testcontainers.PostgreSql`, and `Microsoft.AspNetCore.Mvc.Testing` NuGet packages

## Capabilities

### New Capabilities

- `integration-test-infrastructure`: Shared test fixtures (PostgreSqlFixture, MigrationApplier), database cleanup strategy, RSA key generation, and collection definitions for parallel execution safety
- `auth-integration-tests`: Level 2 handler tests and Level 3 endpoint tests for the Auth bounded context (registration, login, JWT, roles, JWKS)
- `expense-integration-tests`: Level 2 handler tests and Level 3 endpoint tests for the Expense bounded context (CRUD for Categories, Expenses, PaymentMethods, Tags), including real Permission service integration and domain event outbox verification
- `permission-integration-tests`: Level 2 handler tests and Level 3 endpoint tests for the Permission bounded context (role management, permission resolution)

### Modified Capabilities

<!-- No existing capability requirements change. Tests verify existing behavior, they don't modify it. -->

## Impact

- **New projects**: `tests/TestShared`, `tests/Auth.IntegrationTests`, `tests/Expense.IntegrationTests`, `tests/Permission.IntegrationTests`
- **Modified files**: `Directory.Packages.props` (new package versions), `src/Expense/Expense.Infrastructure/Database/ApplicationDbContext.cs` (publish events before save)
- **Dependencies added**: `Testcontainers` (4.5.0), `Testcontainers.PostgreSql` (4.5.0), `Microsoft.AspNetCore.Mvc.Testing` (10.0.7)
- **CI**: Testcontainers requires Docker on CI runners (GitHub-hosted runners have Docker pre-installed)
- **No API changes**: All tests verify existing behavior; no public contracts change
