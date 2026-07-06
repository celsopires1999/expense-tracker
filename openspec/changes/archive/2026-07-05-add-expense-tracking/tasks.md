## 1. Remove Todo system

- [x] 1.1 Delete `src/Domain/Todos/` directory (TodoItem, Priority, errors, 4 domain events)
- [x] 1.2 Delete `src/Application/Todos/` directory (7 sub-namespaces, 20 files)
- [x] 1.3 Delete `src/Infrastructure/Todos/TodoItemConfiguration.cs`
- [x] 1.4 Delete `src/Web.Api/Endpoints/Todos/` directory (6 endpoint files)
- [x] 1.5 Remove `Tags.Todos` constant from `src/Web.Api/Endpoints/Tags.cs`
- [x] 1.6 Remove `DbSet<TodoItem> TodoItems` from `IApplicationDbContext`
- [x] 1.7 Remove `DbSet<TodoItem> TodoItems` from `ApplicationDbContext`
- [x] 1.8 Remove `using Domain.Todos;` from `IApplicationDbContext` and `ApplicationDbContext`
- [x] 1.9 Delete old migration files (`Create_Database.cs`, `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs`)

## 2. Create Domain entities

- [x] 2.1 Create `Domain/Categories/Category.cs` entity (Id, Name)
- [x] 2.2 Create `Domain/PaymentMethods/PaymentMethod.cs` entity (Id, Name)
- [x] 2.3 Create `Domain/Tags/Tag.cs` entity (Id, Name)
- [x] 2.4 Create `Domain/Expenses/Expense.cs` entity (Id, UserId, Description, Amount, Date, CategoryId, PaymentMethodId, CreatedAt, UpdatedAt)
- [x] 2.5 Create `Domain/Expenses/ExpenseErrors.cs` (NotFound)
- [x] 2.6 Create `Domain/Expenses/ExpenseCreatedDomainEvent.cs`
- [x] 2.7 Create `Domain/Expenses/ExpenseUpdatedDomainEvent.cs`
- [x] 2.8 Create `Domain/Expenses/ExpenseDeletedDomainEvent.cs`
- [x] 2.9 Create `Domain/Roles/Role.cs` entity (Id, Name)
- [x] 2.10 Create `Domain/Roles/RolePermission.cs` entity (RoleId, Permission)
- [x] 2.11 Create `Domain/Roles/UserRole.cs` entity (UserId, RoleId)
- [x] 2.12 Add `PermissionVersion` property to `Domain/Users/User.cs`

## 3. Implement permission system (Infrastructure)

- [x] 3.1 Fix `PermissionAuthorizationHandler` — remove early-return stub, implement real cache-first check via `IMemoryCache`
- [x] 3.2 Implement `PermissionProvider.GetForUserIdAsync` — query roles → permissions for user, return `HashSet<string>`
- [x] 3.3 Update `TokenProvider.Create` — read `user.PermissionVersion`, add `perm_version` claim to JWT
- [x] 3.4 Register `IMemoryCache` in DI (built-in, no extra package needed)

## 4. Create Infrastructure layer (EF Config + DbContext + Migration)

- [x] 4.1 Create `Infrastructure/Categories/CategoryConfiguration.cs`
- [x] 4.2 Create `Infrastructure/PaymentMethods/PaymentMethodConfiguration.cs`
- [x] 4.3 Create `Infrastructure/Tags/TagConfiguration.cs`
- [x] 4.4 Create `Infrastructure/Expenses/ExpenseConfiguration.cs` (include ExpenseTag join table config)
- [x] 4.5 Create `Infrastructure/Roles/RoleConfiguration.cs`, `RolePermissionConfiguration.cs`, `UserRoleConfiguration.cs`
- [x] 4.6 Add `DbSet<>` properties to `ApplicationDbContext` and `IApplicationDbContext` for all new entities
- [x] 4.7 Generate new `Create_Database` migration via `dotnet ef migrations add Create_Database`
- [x] 4.8 Add migration seed data (Admin role with all permissions, Viewer role with read permissions, default categories, default payment methods)

## 5. Create Application layer — Expense CQRS

- [x] 5.1 Create `Application/Expenses/Create/` (command, handler, validator)
- [x] 5.2 Create `Application/Expenses/Get/` (query, handler, response DTO)
- [x] 5.3 Create `Application/Expenses/GetById/` (query, handler, response DTO)
- [x] 5.4 Create `Application/Expenses/Update/` (command, handler, validator)
- [x] 5.5 Create `Application/Expenses/Delete/` (command, handler, validator)

## 6. Create Application layer — Reference entities CQRS

- [x] 6.1 Create `Application/Categories/` (GetAll, Create, Update, Delete commands/queries/handlers)
- [x] 6.2 Create `Application/PaymentMethods/` (GetAll, Create, Update, Delete commands/queries/handlers)
- [x] 6.3 Create `Application/Tags/` (GetAll, Create, Update, Delete commands/queries/handlers)

## 7. Create Web Api endpoints

- [x] 7.1 Create `Web.Api/Endpoints/Expenses/Permissions.cs` with permission string constants
- [x] 7.2 Create `Web.Api/Endpoints/Expenses/Create.cs` endpoint (POST /expenses)
- [x] 7.3 Create `Web.Api/Endpoints/Expenses/Get.cs` endpoint (GET /expenses with filters)
- [x] 7.4 Create `Web.Api/Endpoints/Expenses/GetById.cs` endpoint (GET /expenses/{id})
- [x] 7.5 Create `Web.Api/Endpoints/Expenses/Update.cs` endpoint (PUT /expenses/{id})
- [x] 7.6 Create `Web.Api/Endpoints/Expenses/Delete.cs` endpoint (DELETE /expenses/{id})
- [x] 7.7 Create `Web.Api/Endpoints/Categories/` endpoints (GET, POST, PUT, DELETE /categories)
- [x] 7.8 Create `Web.Api/Endpoints/PaymentMethods/` endpoints (GET, POST, PUT, DELETE /payment-methods)
- [x] 7.9 Create `Web.Api/Endpoints/Tags/` endpoints (GET, POST, PUT, DELETE /tags)

## 8. Final cleanup and verification

- [x] 8.1 Remove old `TodoItem` references from any remaining files (manual-tests/api.http, AGENTS.md)
- [x] 8.2 Run `dotnet build ExpenseTracker.slnx` — verify no errors
- [x] 8.3 Run `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — verify layer rules pass
- [x] 8.4 Update `manual-tests/api.http` with new expense endpoints
