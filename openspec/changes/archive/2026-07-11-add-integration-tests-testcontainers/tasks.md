## 1. Foundation — TestShared Project + NuGet Packages

- [x] 1.1 Add `Testcontainers` (4.5.0), `Testcontainers.PostgreSql` (4.5.0), and `Microsoft.AspNetCore.Mvc.Testing` (10.0.7) to `Directory.Packages.props`
- [x] 1.2 Create `tests/TestShared/TestShared.csproj` targeting net10.0 with references to Testcontainers, Testcontainers.PostgreSql, and Microsoft.EntityFrameworkCore
- [x] 1.3 Implement `PostgreSqlFixture.cs` — static lazy `PostgreSqlContainer` (postgres:17, database "integration_tests"), static lazy RSA 2048-bit key pair with temp PEM files, `ConnectionString`/`PrivateKey`/`PublicKey` static properties, `IAsyncLifetime` with cleanup
- [x] 1.4 Implement `MigrationApplier.cs` — generic static `ApplyMigrationsAsync<TContext>(IServiceProvider)` that resolves `IMigrator` and runs `MigrateAsync()`

## 2. Auth Integration Tests — Project + Fixtures

- [x] 2.1 Create `tests/Auth.IntegrationTests/Auth.IntegrationTests.csproj` with references to TestShared, Auth.Api, and Microsoft.AspNetCore.Mvc.Testing
- [x] 2.2 Create `GlobalUsings.cs` with global xUnit using
- [x] 2.3 Create `CollectionDefinitions.cs` with `[assembly: CollectionBehavior(DisableTestParallelization = true)]` and `[CollectionDefinition("Auth")]`
- [x] 2.4 Implement `AuthApiFixture.cs` — `WebApplicationFactory<Program>` overriding ConnectionStrings:Database, Jwt:PrivateKeyPath, Jwt:PublicKeyPath, Jwt:Issuer, Jwt:Audience, Jwt:ExpirationInMinutes; removing Serilog; running MigrationApplier for AuthDbContext; seeding "Standard" role
- [x] 2.5 Implement `Helpers/AuthTestHelper.cs` — helper to create test users via handler, get JWT tokens, create authenticated `HttpClient` with Bearer header

## 3. Auth Integration Tests — Handler Tests (Level 2)

- [x] 3.1 Create `Handlers/RegisterUserCommandTests.cs` — 7 tests: creates user, hashes password, assigns Standard role, rejects duplicate email, validates empty email, validates short password, validates empty first name
- [x] 3.2 Create `Handlers/LoginUserCommandTests.cs` — 5 tests: returns JWT for valid credentials, rejects unknown email, rejects wrong password, JWT contains correct claims, JWT is verifiable with public key
- [x] 3.3 Create `Handlers/GetUserByIdQueryTests.cs` — 3 tests: returns user response, excludes password hash, returns not found for unknown id
- [x] 3.4 Create `Handlers/GetUserRolesQueryTests.cs` — 2 tests: returns role names, returns empty array for no roles
- [x] 3.5 Create `Handlers/AssignRoleCommandTests.cs` — 4 tests: creates UserRole record, idempotent double-assign, error for unknown user, error for unknown role
- [x] 3.6 Create `Handlers/RemoveRoleCommandTests.cs` — 2 tests: deletes UserRole record, idempotent remove of non-existent role

## 4. Auth Integration Tests — Endpoint Tests (Level 3)

- [x] 4.1 Create `Endpoints/RegisterEndpointTests.cs` — 4 tests: 201 Created with Guid, 400 Bad Request invalid email, 400 Bad Request short password, 409 Conflict duplicate email
- [x] 4.2 Create `Endpoints/LoginEndpointTests.cs` — 2 tests: 200 OK with token, 404 Not Found wrong credentials
- [x] 4.3 Create `Endpoints/GetUserByIdEndpointTests.cs` — 4 tests: 200 OK with valid token, 401 Unauthorized without token, 401 Unauthorized invalid token, 404 Not Found unknown id
- [x] 4.4 Create `Endpoints/AssignRoleEndpointTests.cs` — 2 tests: 200 OK with valid token, 401 Unauthorized without token
- [x] 4.5 Create `Endpoints/RemoveRoleEndpointTests.cs` — 2 tests: 200 OK with valid token, 401 Unauthorized without token
- [x] 4.6 Create `Endpoints/JwksEndpointTests.cs` — 2 tests: 200 OK with valid JWKS JSON, key can verify token from login
- [x] 4.7 Run `dotnet test` and verify all Auth tests pass

## 5. Permission Integration Tests — Project + Fixtures

- [x] 5.1 Create `tests/Permission.IntegrationTests/Permission.IntegrationTests.csproj` with references to TestShared, Permission.Api, and Microsoft.AspNetCore.Mvc.Testing
- [x] 5.2 Create `GlobalUsings.cs` and `CollectionDefinitions.cs` (disable parallel)
- [x] 5.3 Implement `PermissionApiFixture.cs` — `WebApplicationFactory<Program>` overriding ConnectionStrings:Database, Jwt:Issuer, Jwt:Audience, Jwt:JwksUrl (to "http://unused"); removing Serilog; running MigrationApplier for PermissionDbContext; seeding roles and permissions
- [x] 5.4 Implement `Helpers/PermissionTestHelper.cs` — helper to seed roles and RolePermissions records in the Permission database

## 6. Permission Integration Tests — Tests

- [x] 6.1 Create `Handlers/CreateRoleCommandTests.cs` — 2 tests: creates role, rejects duplicate name
- [x] 6.2 Create `Handlers/ResolvePermissionsQueryTests.cs` — 2 tests: returns permissions for roles, returns empty set for unknown roles
- [x] 6.3 Create `Handlers/UpdateRolePermissionsCommandTests.cs` — 1 test: sets permissions for role
- [x] 6.4 Create `Handlers/DeleteRoleCommandTests.cs` — 2 tests: removes role, error for unknown role
- [x] 6.5 Create `Handlers/GetRolesQueryTests.cs` — 1 test: returns all roles
- [x] 6.6 Create `Endpoints/CreateRoleEndpointTests.cs` — 2 tests: 201 with valid token, 401 without token
- [x] 6.7 Create `Endpoints/ResolvePermissionsEndpointTests.cs` — 2 tests: 200 OK with roles in body (anonymous), 200 OK with empty roles
- [x] 6.8 Create `Endpoints/UpdateRolePermissionsEndpointTests.cs` — 2 tests: 200 OK with valid token, 401 without token
- [x] 6.9 Create `Endpoints/DeleteRoleEndpointTests.cs` — 2 tests: 204 No Content with valid token, 401 without token
- [x] 6.10 Create `Endpoints/GetRolesEndpointTests.cs` — 2 tests: 200 OK with valid token, 401 without token
- [x] 6.11 Run `dotnet test` and verify all Permission tests pass

## 7. Expense Integration Tests — Project + Fixtures

- [x] 7.1 Create `tests/Expense.IntegrationTests/Expense.IntegrationTests.csproj` with references to TestShared, Expense.Api, Permission.Application, Permission.Infrastructure, Auth.Application, Auth.Infrastructure, and Microsoft.AspNetCore.Mvc.Testing
- [x] 7.2 Create `GlobalUsings.cs` and `CollectionDefinitions.cs` (disable parallel)
- [x] 7.3 Implement `ExpenseApiFixture.cs` — two `WebApplicationFactory` instances (Expense + Permission); Expense factory overrides ConnectionStrings:Database, Jwt:*, PermissionService:BaseUrl pointing to Permission factory's TestServer; Permission factory overrides its ConnectionStrings:Database and Jwt:JwksUrl; both run MigrationApplier; Permission factory seeds roles + permissions
- [x] 7.4 Implement `Helpers/ExpenseTestHelper.cs` — helpers to create users in Auth DB, create JWT tokens, seed categories/payment methods/tags in Expense DB, seed roles + permissions in Permission DB, create authenticated HttpClient

## 8. Expense Integration Tests — Handler Tests (Level 2)

- [x] 8.1 Create `Handlers/CreateCategoryCommandTests.cs` — tests: creates category, rejects duplicate name
- [x] 8.2 Create `Handlers/UpdateCategoryCommandTests.cs` — tests: updates category, rejects duplicate name, returns not found
- [x] 8.3 Create `Handlers/DeleteCategoryCommandTests.cs` — tests: deletes category, rejects category in use, returns not found
- [x] 8.4 Create `Handlers/GetAllCategoriesQueryTests.cs` — tests: returns all categories ordered by name
- [x] 8.5 Create `Handlers/CreateExpenseCommandTests.cs` — tests: creates expense, raises ExpenseCreatedDomainEvent, validates referenced entities exist, enforces ownership
- [x] 8.6 Create `Handlers/UpdateExpenseCommandTests.cs` — tests: updates expense, raises ExpenseUpdatedDomainEvent, enforces ownership
- [x] 8.7 Create `Handlers/DeleteExpenseCommandTests.cs` — tests: deletes expense, raises ExpenseDeletedDomainEvent, enforces ownership
- [x] 8.8 Create `Handlers/GetExpensesQueryTests.cs` — tests: returns expenses, filters by userId/date/category/tag
- [x] 8.9 Create `Handlers/GetExpenseByIdQueryTests.cs` — tests: returns expense, returns not found
- [x] 8.10 Create `Handlers/CreatePaymentMethodCommandTests.cs` — tests: creates payment method, rejects duplicate name
- [x] 8.11 Create `Handlers/UpdatePaymentMethodCommandTests.cs` — tests: updates payment method, rejects duplicate name
- [x] 8.12 Create `Handlers/DeletePaymentMethodCommandTests.cs` — tests: deletes payment method, rejects in use
- [x] 8.13 Create `Handlers/GetAllPaymentMethodsQueryTests.cs` — tests: returns all payment methods
- [x] 8.14 Create `Handlers/CreateTagCommandTests.cs` — tests: creates tag, rejects duplicate name
- [x] 8.15 Create `Handlers/UpdateTagCommandTests.cs` — tests: updates tag, rejects duplicate name
- [x] 8.16 Create `Handlers/DeleteTagCommandTests.cs` — tests: deletes tag, rejects in use
- [x] 8.17 Create `Handlers/GetAllTagsQueryTests.cs` — tests: returns all tags
- [x] 8.18 Create `Handlers/DomainEventTests.cs` — 3 tests: outbox captures ExpenseCreated/Updated/Deleted events with correct payload

## 9. Expense Integration Tests — Endpoint Tests (Level 3)

- [x] 9.1 Create `Endpoints/CreateCategoryEndpointTests.cs` — tests: 201 with valid permissions, 401 without token, 400 invalid data
- [x] 9.2 Create `Endpoints/UpdateCategoryEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.3 Create `Endpoints/DeleteCategoryEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.4 Create `Endpoints/GetAllCategoriesEndpointTests.cs` — tests: 200 with valid permissions, 401 without token
- [x] 9.5 Create `Endpoints/CreateExpenseEndpointTests.cs` — tests: 201 with valid permissions, 401 without token, 400 invalid data
- [x] 9.6 Create `Endpoints/UpdateExpenseEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.7 Create `Endpoints/DeleteExpenseEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.8 Create `Endpoints/GetExpensesEndpointTests.cs` — tests: 200 with valid permissions, 401 without token
- [x] 9.9 Create `Endpoints/GetExpenseByIdEndpointTests.cs` — tests: 200 with valid permissions, 401 without token, 404 not found
- [x] 9.10 Create `Endpoints/CreatePaymentMethodEndpointTests.cs` — tests: 201 with valid permissions, 401 without token
- [x] 9.11 Create `Endpoints/UpdatePaymentMethodEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.12 Create `Endpoints/DeletePaymentMethodEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.13 Create `Endpoints/GetAllPaymentMethodsEndpointTests.cs` — tests: 200 with valid permissions, 401 without token
- [x] 9.14 Create `Endpoints/CreateTagEndpointTests.cs` — tests: 201 with valid permissions, 401 without token
- [x] 9.15 Create `Endpoints/UpdateTagEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.16 Create `Endpoints/DeleteTagEndpointTests.cs` — tests: 204 with valid permissions, 401 without token
- [x] 9.17 Create `Endpoints/GetAllTagsEndpointTests.cs` — tests: 200 with valid permissions, 401 without token
- [x] 9.18 Run `dotnet test` and verify all Expense tests pass

## 10. Domain Event Outbox Fix

- [x] 10.1 Modify `ApplicationDbContext` constructor to accept `IPublishEndpoint` in addition to existing dependencies
- [x] 10.2 Modify `ApplicationDbContext.SaveChangesAsync` to publish domain events to `IPublishEndpoint.Publish()` BEFORE calling `base.SaveChangesAsync()`
- [x] 10.3 Verify outbox verification tests in `DomainEventTests.cs` pass
- [x] 10.4 Run full `dotnet test` across all test projects and verify no regressions

## 11. Final Verification

- [x] 11.1 Run `dotnet build` on the solution and verify no build errors or warnings
- [x] 11.2 Run `dotnet test` on all test projects and verify all tests pass
- [x] 11.3 Verify Testcontainers containers are cleaned up after test run (Ryuk)
