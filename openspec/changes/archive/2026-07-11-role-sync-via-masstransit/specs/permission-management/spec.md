## MODIFIED Requirements

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
