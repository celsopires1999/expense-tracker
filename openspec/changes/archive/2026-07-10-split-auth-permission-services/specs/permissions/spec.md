## MODIFIED Requirements

### Requirement: JWT contains permission version
The system SHALL include `perm_version` claim in the JWT at login time. The value SHALL be read from `User.PermissionVersion`.

→ **REPLACED BY**: The Auth Service SHALL issue JWTs containing `roles` (array of role names) instead of `perm_version`. Permission resolution is delegated to the Permission Service via HTTP. The Expense Tracker SHALL cache resolved permissions by user ID with TTL-based expiry.

#### Scenario: Login includes perm_version
→ **REPLACED BY**:

#### Scenario: Login JWT contains roles
- **WHEN** a user logs in successfully via the Auth Service
- **THEN** the JWT contains a `roles` claim with the user's assigned role names (e.g., `["Standard"]`)
- **THEN** the JWT does NOT contain `perm_version`

### Requirement: Permission check reads from cache
The `PermissionAuthorizationHandler` SHALL resolve permissions from `IMemoryCache` keyed by `perms:{userId}:v{version}`.

→ **REPLACED BY**: The Expense Tracker SHALL resolve permissions by calling the Permission Service's `POST /permissions/resolve` endpoint, passing the user's roles from the JWT. Results SHALL be cached in `IMemoryCache` keyed by `perms:{userId}` with a configurable TTL (default: 5 minutes).

#### Scenario: Permission cache hit
→ **REPLACED BY**:

#### Scenario: Cached permission allows request
- **WHEN** a user makes a request to an endpoint requiring `expenses:create`
- **AND** the cache contains `perms:{userId}` with `expenses:create` in the set
- **AND** the cache entry is not expired
- **THEN** the request is authorized

#### Scenario: Permission cache miss loads from Permission Service
→ **REPLACED BY**:

#### Scenario: Cache miss resolves via Permission Service
- **WHEN** a user makes a request
- **AND** the cache does NOT contain `perms:{userId}` OR the entry is expired
- **THEN** the Expense Tracker extracts `roles` from the JWT
- **THEN** the Expense Tracker calls `POST /permissions/resolve` with `{ roles: ["Standard"] }`
- **THEN** the result is cached with a fresh TTL
- **THEN** the request is authorized or denied based on the result

#### Scenario: User lacks required permission
- **WHEN** a user makes a request to an endpoint requiring `expenses:read:all`
- **AND** the resolved permissions do not include `expenses:read:all`
- **THEN** the response returns 403 Forbidden

### Requirement: Permission cache invalidated on version change
The system SHALL detect cache staleness via the `perm_version` claim. If the cache key does not match the current version, the cache is bypassed and refreshed from DB.

→ **REMOVED**. Cache invalidation is handled by TTL expiry only. Permission changes propagate within the TTL window. No versioned cache keys or cross-service invalidation events.

**Reason**: Eliminated cross-service notification complexity. The TTL window (5 minutes) is acceptable for permission propagation.

**Migration**: Remove `perm_version` from `User` entity in Auth Service. Remove versioned cache key construction. Configure TTL in Expense Tracker's caching layer.

### Requirement: PermissionProvider queries user's effective permissions
The `PermissionProvider.GetForUserIdAsync` SHALL return the union of all permissions from all roles assigned to the user.

→ **MOVED TO** Permission Service (`POST /permissions/resolve`). The Permission Service SHALL accept role names (not user ID) and return the union of their permissions.

#### Scenario: User has multiple roles
→ **MOVED TO**:

#### Scenario: Resolve multiple roles returns union
- **WHEN** a user has both Admin and Viewer roles
- **AND** the Expense Tracker calls `POST /permissions/resolve` with `{ roles: ["Admin", "Viewer"] }`
- **THEN** the Permission Service returns the union of both roles' permissions

#### Scenario: User has no explicit roles
→ **MOVED TO**:

#### Scenario: Empty role list returns empty set
- **WHEN** a user has no assigned roles (empty `roles` array in JWT)
- **AND** the Expense Tracker calls `POST /permissions/resolve` with `{ roles: [] }`
- **THEN** the Permission Service returns an empty set
- **THEN** the user has no permissions

### Requirement: Seed Admin role with all permissions
The migration SHALL seed an Admin role with all available permissions.

→ **MOVED TO** Permission Service migration (seeded in Permission database, not Expense Tracker database).

#### Scenario: Admin role permissions pre-seeded
- **WHEN** the Permission Service database migration runs
- **THEN** there is a role named "Admin" with permissions for all CRUD operations on expenses, categories, payment methods, tags, and users

### Requirement: Seed Viewer role with read-only permissions
The migration SHALL seed a Viewer role with read permissions across all expenses and reference entities.

→ **MOVED TO** Permission Service migration.

#### Scenario: Viewer role permissions pre-seeded
- **WHEN** the Permission Service database migration runs
- **THEN** there is a role named "Viewer" with permissions: expenses:read, expenses:read:all, categories:read, payment-methods:read, tags:read, users:access
