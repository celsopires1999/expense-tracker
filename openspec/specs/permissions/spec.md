# Permissions

## Purpose
Define the role-based permission system that controls access to expense tracking features, supporting fine-grated authorization via JWT claims and cached HTTP resolution against the Permission Service.

## Requirements

### Requirement: JWT contains roles claim
The Auth Service SHALL issue JWTs containing `roles` (array of role names) instead of `perm_version`. Permission resolution is delegated to the Permission Service via HTTP. The Expense Tracker SHALL cache resolved permissions by user ID with TTL-based expiry.

#### Scenario: Login JWT contains roles
- **WHEN** a user logs in successfully via the Auth Service
- **THEN** the JWT contains a `roles` claim with the user's assigned role names (e.g., `["Standard"]`)
- **THEN** the JWT does NOT contain `perm_version`

### Requirement: Permission check resolves via Permission Service with caching
The Expense Tracker SHALL resolve permissions by calling the Permission Service's `POST /permissions/resolve` endpoint, passing the user's roles from the JWT. Results SHALL be cached in `IMemoryCache` keyed by `perms:{userId}` with a configurable TTL (default: 5 minutes).

#### Scenario: Cached permission allows request
- **WHEN** a user makes a request to an endpoint requiring `expenses:create`
- **AND** the cache contains `perms:{userId}` with `expenses:create` in the set
- **AND** the cache entry is not expired
- **THEN** the request is authorized

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

### Requirement: Resolve multiple roles returns union
The Permission Service SHALL return the union of permissions from all role names passed to `POST /permissions/resolve`.

#### Scenario: Resolve multiple roles returns union
- **WHEN** a user has both Admin and Viewer roles
- **AND** the Expense Tracker calls `POST /permissions/resolve` with `{ roles: ["Admin", "Viewer"] }`
- **THEN** the Permission Service returns the union of both roles' permissions

#### Scenario: Empty role list returns empty set
- **WHEN** a user has no assigned roles (empty `roles` array in JWT)
- **AND** the Expense Tracker calls `POST /permissions/resolve` with `{ roles: [] }`
- **THEN** the Permission Service returns an empty set
- **THEN** the user has no permissions
