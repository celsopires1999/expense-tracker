## Why

The current codebase has a generic Todo scaffold, but the actual product is an ExpenseTracker. We need to replace the Todo domain with a rich Expense domain and build a proper permission system to support three user roles (default user, Admin, Viewer). The existing permission scaffolding (`HasPermissionAttribute`, `PermissionAuthorizationHandler`) is stubbed out and must be made functional.

## What Changes

**BREAKING**: Replace `TodoItem` entity and all related code with `Expense` domain.
- Remove `Domain/Todos/` (TodoItem, Priority, errors, 4 domain events)
- Remove `Application/Todos/` (20 files across 7 sub-namespaces)
- Remove `Infrastructure/Todos/TodoItemConfiguration.cs`
- Remove `Web.Api/Endpoints/Todos/` (6 endpoint files)
- Remove `todo_items` table and related migration artifacts

**New Domain Entities**:
- `Expense` — core entity with Description, Amount, Date, CategoryId, PaymentMethodId, Tags (M:N)
- `Category` — shared reference entity (seeded: Alimentação, Transporte, etc.)
- `PaymentMethod` — shared reference entity (seeded: Crédito, Débito, Dinheiro, Pix, etc.)
- `Tag` — global reference entity (M:N with Expense via ExpenseTag)
- `Role`, `RolePermission`, `UserRole` — authorization model
- `User.PermissionVersion` — new column for cache invalidation strategy

**New Permission System**:
- Roles seed (Admin, Viewer) + implicit default role for all authenticated users
- Permission strings defined in database, not hardcoded (except constants for endpoint use)
- JWT carries `perm_version` (int) instead of full permission list
- `PermissionAuthorizationHandler` reads from `IMemoryCache`; cache miss queries DB via `PermissionProvider`
- Revocation by incrementing `perm_version` on the User row

## Capabilities

### New Capabilities

- `expense-management`: CRUD of expenses by owners; Admin CRUD of any expense; Viewer read-only across all expenses
- `expense-categories`: Shared reference categories, CRUD by Admin only, read by all
- `payment-methods`: Shared reference payment methods, CRUD by Admin only, read by all
- `tags`: Global tags with M:N relationship to expenses, CRUD by Admin only, read by all
- `permissions`: Role-based permission system with versioned JWT cache invalidation

### Modified Capabilities

*(None — no existing specs to modify)*

## Impact

- **Code**: ~39 files removed (Todo), ~50+ files created (new domain entities, application layer, endpoints, infrastructure config, migration)
- **Database**: New migration drops `todo_items`, creates `expenses`, `categories`, `payment_methods`, `tags`, `expense_tag`, `roles`, `role_permissions`, `user_roles` tables; adds `permission_version` column to `users`
- **Auth**: JWT payload changes (adds `perm_version` claim); `PermissionAuthorizationHandler` and `PermissionProvider` become functional
- **Infrastructure**: `IMemoryCache` added to DI (in-process, no Redis dependency)
- **API**: All `/todos` endpoints replaced by `/expenses` endpoints; new `/categories`, `/payment-methods`, `/tags`, `/users` endpoints added
