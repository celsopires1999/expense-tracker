# Permission Management

## Purpose
Define the Permission Service's responsibilities for role CRUD, role-to-permission mapping, and resolving role names to permission sets. The Permission Service is a standalone service that owns permission definitions and resolution logic.

## Requirements

### Requirement: Permission Service manages role definitions
The Permission Service SHALL store and manage roles with deterministic GUIDs matching the Auth Service. Role lifecycle changes (create, update, delete) SHALL publish events via MassTransit for cross-context synchronization.

#### Scenario: Roles seeded on migration
- **WHEN** the Permission Service database is migrated
- **THEN** three roles exist: Admin (11111111-1111-1111-1111-111111111111), Viewer (22222222-2222-2222-2222-222222222222), Standard (33333333-3333-3333-3333-333333333333)

#### Scenario: Admin creates a new role
- **WHEN** an authorized admin sends `POST /permissions/roles` with a unique role name
- **THEN** a new role is created
- **THEN** a `RoleCreatedEvent` is published via MassTransit
- **THEN** the response returns 201 with the role's GUID

#### Scenario: Admin updates a role
- **WHEN** an authorized admin sends `PUT /permissions/roles/{roleId}` to update a role's name
- **THEN** the role's name is updated
- **THEN** a `RoleUpdatedEvent` is published via MassTransit
- **THEN** the response returns 200

#### Scenario: Admin deletes a role
- **WHEN** an authorized admin sends `DELETE /permissions/roles/{roleId}`
- **THEN** the role and its role-permission assignments are deleted
- **THEN** a `RoleDeletedEvent` is published via MassTransit
- **THEN** the response returns 200

### Requirement: Permission Service manages role-to-permission mapping
The Permission Service SHALL allow authorized clients to assign and remove permissions for each role.

#### Scenario: Assign permissions to a role
- **WHEN** an authorized admin sends `PUT /permissions/roles/{roleId}/permissions` with a list of permission strings
- **THEN** the role's permissions are replaced with the provided list
- **THEN** the response returns 200

#### Scenario: Read permissions for a role
- **WHEN** a client sends `GET /permissions/roles/{roleId}`
- **THEN** the response returns 200 with the role's name and its permissions

#### Scenario: Assign permission to nonexistent role
- **WHEN** an authorized admin sends `PUT /permissions/roles/{nonexistentId}/permissions`
- **THEN** the response returns 404 Not Found

### Requirement: Permission Service resolves role names to permission sets
The Permission Service SHALL expose `POST /permissions/resolve` that accepts a set of role names and returns the union of their permissions.

#### Scenario: Resolve single role
- **WHEN** a client sends `POST /permissions/resolve` with `{ "roles": ["Standard"] }`
- **THEN** the response returns 200 with the permissions assigned to the Standard role

#### Scenario: Resolve multiple roles (union)
- **WHEN** a client sends `POST /permissions/resolve` with `{ "roles": ["Admin", "Viewer"] }`
- **THEN** the response returns 200 with the union of Admin's and Viewer's permissions

#### Scenario: Resolve empty role list
- **WHEN** a client sends `POST /permissions/resolve` with `{ "roles": [] }`
- **THEN** the response returns 200 with an empty permissions array

#### Scenario: Resolve with nonexistent role name
- **WHEN** a client sends `POST /permissions/resolve` with a role name that does not exist
- **THEN** the response returns 200 — nonexistent role names are silently ignored and contribute no permissions

### Requirement: Permissions use namespaced string format
Permissions SHALL follow the format `{resource}:{action}` (e.g., `expense:create`, `category:read`) to support multi-application coexistence.

#### Scenario: Namespace isolation
- **WHEN** an Expense Tracker permission `expense:create` and an App X permission `order:create` are both assigned to the same role
- **THEN** they coexist without collision

### Requirement: Permissions are seeded for Admin, Viewer, Standard roles
The Permission Service SHALL seed default permissions for each role on database migration.

#### Scenario: Admin role has all permissions
- **WHEN** the database migration runs
- **THEN** the Admin role has permissions for all CRUD operations on expenses, categories, payment-methods, tags, and users

#### Scenario: Viewer role has read-only permissions
- **WHEN** the database migration runs
- **THEN** the Viewer role has permissions: expenses:read, expenses:read:all, categories:read, payment-methods:read, tags:read, users:access

#### Scenario: Standard role has own-resource permissions
- **WHEN** the database migration runs
- **THEN** the Standard role has permissions: expenses:create, expenses:read, expenses:update, expenses:delete, categories:read, payment-methods:read, tags:read, users:access
