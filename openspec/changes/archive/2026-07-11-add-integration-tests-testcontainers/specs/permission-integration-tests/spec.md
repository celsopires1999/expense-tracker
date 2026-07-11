## ADDED Requirements

### Requirement: Permission handler-level integration tests
The Permission integration test project SHALL verify all CQRS handlers against a real PostgreSQL database.

#### Scenario: CreateRoleCommand creates role
- **WHEN** `CreateRoleCommandHandler` handles a valid command
- **THEN** a role record exists in the database with the provided name

#### Scenario: CreateRoleCommand rejects duplicate name
- **WHEN** `CreateRoleCommandHandler` handles a command with an existing role name
- **THEN** the result is a Conflict error

#### Scenario: ResolvePermissionsQuery returns permissions for roles
- **WHEN** `ResolvePermissionsQueryHandler` handles a query with role names that have permissions assigned
- **THEN** the result contains the distinct set of permission strings for those roles

#### Scenario: ResolvePermissionsQuery returns empty set for unknown roles
- **WHEN** `ResolvePermissionsQueryHandler` handles a query with role names that don't exist
- **THEN** the result is an empty set

#### Scenario: UpdateRolePermissionsCommand sets permissions
- **WHEN** `UpdateRolePermissionsCommandHandler` handles a valid command
- **THEN** the role's permissions are replaced with the provided list

#### Scenario: DeleteRoleCommand removes role
- **WHEN** `DeleteRoleCommandHandler` handles a valid command for an existing role
- **THEN** the role record is removed from the database

#### Scenario: GetRolesQuery returns all roles
- **WHEN** `GetRolesQueryHandler` handles a query
- **THEN** the result contains all roles in the database

### Requirement: Permission endpoint-level integration tests
The Permission integration test project SHALL verify all HTTP endpoints via `WebApplicationFactory<Program>`.

#### Scenario: POST /permissions/roles requires authentication
- **WHEN** a request without a JWT token is sent to `POST /permissions/roles`
- **THEN** the response is 401 Unauthorized

#### Scenario: POST /permissions/roles returns 201 with valid token
- **WHEN** a request with a valid JWT token is sent to `POST /permissions/roles`
- **THEN** the response is 201 Created with the new role GUID

#### Scenario: POST /permissions/resolve is anonymous
- **WHEN** a request without a JWT token is sent to `POST /permissions/resolve` with role names in the body
- **THEN** the response is 200 OK with the resolved permissions
- **AND** the endpoint does not require authentication

#### Scenario: PUT /permissions/roles/{id}/permissions updates permissions
- **WHEN** a request with a valid JWT token and a list of permissions is sent to `PUT /permissions/roles/{id}/permissions`
- **THEN** the response is 200 OK
- **AND** the role's permissions are updated

#### Scenario: DELETE /permissions/roles/{id} removes role
- **WHEN** a request with a valid JWT token is sent to `DELETE /permissions/roles/{id}`
- **THEN** the response is 204 No Content
- **AND** the role is removed from the database

#### Scenario: GET /permissions/roles returns all roles
- **WHEN** a request with a valid JWT token is sent to `GET /permissions/roles`
- **THEN** the response is 200 OK with a list of roles
