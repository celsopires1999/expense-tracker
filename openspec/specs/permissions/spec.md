# Permissions

## Purpose
Define the role-based permission system that controls access to expense tracking features, supporting fine-grained authorization via JWT claims and cached database lookups.

## Requirements

### Requirement: JWT contains permission version
The system SHALL include `perm_version` claim in the JWT at login time. The value SHALL be read from `User.PermissionVersion`.

#### Scenario: Login includes perm_version
- **WHEN** a user logs in successfully
- **THEN** the JWT contains a `perm_version` claim with the user's current permission version

### Requirement: Permission check reads from cache
The `PermissionAuthorizationHandler` SHALL resolve permissions from `IMemoryCache` keyed by `perms:{userId}:v{version}`.

#### Scenario: Permission cache hit
- **WHEN** a user makes a request to an endpoint with `[HasPermission("expenses:create")]`
- **AND** the cache contains `perms:{userId}:v{version}` with "expenses:create" in the set
- **THEN** the request is authorized (200 or appropriate success)

#### Scenario: Permission cache miss loads from DB
- **WHEN** a user makes a request
- **AND** the cache does NOT contain `perms:{userId}:v{version}`
- **THEN** `PermissionProvider.GetForUserIdAsync` queries the database
- **THEN** the result is cached
- **THEN** the request is authorized or denied based on the result

#### Scenario: User lacks required permission
- **WHEN** a user makes a request to an endpoint requiring "expenses:read:all"
- **AND** the user's permissions do not include "expenses:read:all"
- **THEN** the response returns 403 Forbidden

### Requirement: Permission cache invalidated on version change
The system SHALL detect cache staleness via the `perm_version` claim. If the cache key does not match the current version, the cache is bypassed and refreshed from DB.

#### Scenario: Admin revokes permission
- **WHEN** an Admin increments a user's `PermissionVersion`
- **AND** the user makes a subsequent request with the old JWT
- **THEN** the cache key `perms:{userId}:v{old}` results in a miss
- **THEN** the new permissions are loaded from DB and cached under `perms:{userId}:v{new}`
- **THEN** the request is authorized based on new permissions

### Requirement: PermissionProvider queries user's effective permissions
The `PermissionProvider.GetForUserIdAsync` SHALL return the union of all permissions from all roles assigned to the user.

#### Scenario: User has multiple roles
- **WHEN** a user has both Admin and Viewer roles
- **THEN** `PermissionProvider` returns the union of both roles' permissions

#### Scenario: User has no explicit roles
- **WHEN** a user has no rows in `UserRole`
- **THEN** `PermissionProvider` returns an empty set
- **THEN** the user is a "default user" with only implicit permissions (expenses:create, read, update, delete own)

### Requirement: Seed Admin role with all permissions
The migration SHALL seed an Admin role with all available permissions.

#### Scenario: Admin role permissions pre-seeded
- **WHEN** the database migration runs
- **THEN** there is a role named "Admin" with permissions for all CRUD operations on expenses, categories, payment methods, tags, and users

### Requirement: Seed Viewer role with read-only permissions
The migration SHALL seed a Viewer role with read permissions across all expenses and reference entities.

#### Scenario: Viewer role permissions pre-seeded
- **WHEN** the database migration runs
- **THEN** there is a role named "Viewer" with permissions: expenses:read, expenses:read:all, categories:read, payment-methods:read, tags:read, users:access
