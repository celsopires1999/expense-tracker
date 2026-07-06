## Context

The project currently has a TodoItem scaffold (entity, commands, handlers, endpoints, EF config, migration) that was used as a development pattern placeholder. The real product is an ExpenseTracker. The Todo system must be fully removed and replaced with an Expense domain.

The existing authorization scaffold (`HasPermissionAttribute`, `PermissionAuthorizationHandler`, `PermissionProvider`) exists but is non-functional — the handler short-circuits for any authenticated user and `PermissionProvider` returns an empty set. This change makes it operational.

The permission model uses versioned JWT + `IMemoryCache` to avoid both oversized JWTs and DB roundtrips on every request. This is an in-process cache (no Redis dependency).

## Goals / Non-Goals

**Goals:**
- Replace TodoItem with Expense as the core domain entity
- Add Category, PaymentMethod, and Tag as supporting reference entities
- Implement a role-based permission system with three roles: default user, Admin, Viewer
- Make `PermissionAuthorizationHandler` functional: read permissions from cache/DB, validate against requirement
- Add `perm_version` to User entity and JWT claims for cache invalidation
- Seed Admin and Viewer roles with their respective permission sets
- Expose CRUD endpoints for expenses (scoped by role) and management endpoints for reference entities

**Non-Goals:**
- No Redis or external cache — `IMemoryCache` only
- No UI/frontend changes
- No audit logging (future consideration)
- No soft-delete (hard delete for MVP)
- No recurring expenses or installment splitting

## Decisions

**Permission version in JWT instead of full permission list**
- JWT stays small (only `sub`, `email`, `perm_version`)
- `IMemoryCache` caches `perms:{userId}:v{version}` → string[]
- Cache miss → `PermissionProvider` queries DB → populates cache
- Revocation: increment `User.PermissionVersion` → next request gets cache miss → refreshes
- No Redis needed (monolith, single process)

**Role ↔ Permission mapping in DB, not hardcoded**
- `Role`, `RolePermission`, `UserRole` tables
- Roles seeded via migration (Admin, Viewer)
- No "default user" role in DB — implicit: any authenticated user without Admin/Viewer role is a default user
- `PermissionProvider` joins roles → permissions for the user
- Permission strings still defined as constants in `Web.Api/Endpoints/Expenses/Permissions.cs` for endpoint usage (compile-time safety)

**Category, PaymentMethod, Tag as shared/global reference entities**
- All three are seeded via migration with initial data
- All users share the same set
- Admin manages CRUD; others read-only
- Tag is M:N with Expense via ExpenseTag join table

**Expense entity design**
- No aggregate root base class — follows existing pattern (Entity only)
- Domain events raised by command handlers, not entity methods (existing pattern)
- Business rules enforced in command handlers + FluentValidation
- `Date` stored as `DateOnly` (PG `date`); `Amount` as `decimal(18,2)`

**File structure mirrors existing pattern**
```
Domain/Expenses/Expense.cs, ExpenseErrors.cs, ExpenseCreatedDomainEvent.cs, ...
Domain/Categories/Category.cs ...
Domain/PaymentMethods/PaymentMethod.cs ...
Domain/Tags/Tag.cs ...
Domain/Roles/Role.cs, RolePermission.cs, UserRole.cs ...
```

**Migration strategy**
- Remove old migration files for `Create_Database` (up/down/designer/snapshot)
- Create a single new migration `Create_Database` that creates all tables from scratch
- Seed roles, permissions, default categories, default payment methods via migration

## Risks / Trade-offs

- **IMemoryCache is per-instance**: If the monolith scales to multiple instances in the future, each instance has a stale cache. Mitigation: version bump causes cache miss on next request; worst case is one request with stale permissions per instance. Future: swap to Redis distributed cache.
- **Permission claim not in JWT**: If the token is decoded by another service (future), it cannot read permissions. Mitigation: not needed — monolith validates internally.
- **Removing old migration**: Destructive for existing databases. Mitigation: we are pre-MVP, no production data exists yet.
- **Hard delete of expenses**: No recovery if accidentally deleted. Mitigation: MVP scope; soft-delete can be added later.
