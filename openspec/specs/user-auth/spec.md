# User Auth

## Purpose
Define the Auth Service's responsibilities for user registration, login, JWT issuance, and role assignment. The Auth Service is a standalone service that owns user identity and authentication.

## Requirements

### Requirement: User can register with email and password
The Auth Service SHALL allow new users to register with an email and password. The password SHALL be hashed using PBKDF2-SHA512 with a 16-byte salt and 500,000 iterations.

#### Scenario: Successful registration
- **WHEN** a client sends `POST /auth/register` with a valid email and password
- **THEN** a new User is created with a hashed password
- **THEN** a `UserRole` record is created linking the user to the "Standard" role
- **THEN** the response returns 201 Created
- **THEN** the Auth Service publishes a `UserCreated` event via MassTransit

#### Scenario: Duplicate email registration
- **WHEN** a client sends `POST /auth/register` with an email already in use
- **THEN** the response returns 409 Conflict with an error message

#### Scenario: Invalid email format
- **WHEN** a client sends `POST /auth/register` with an invalid email format
- **THEN** the response returns 400 Bad Request with validation errors

### Requirement: User can log in and receive a JWT
The Auth Service SHALL authenticate users by email and password, issuing an RSA-signed JWT with `sub`, `email`, and `roles` claims.

#### Scenario: Successful login with Standard role
- **WHEN** a registered user sends `POST /auth/login` with valid credentials
- **AND** the user has the "Standard" role assigned
- **THEN** the response returns 200 with a JWT containing `sub` (user ID), `email`, and `roles: ["Standard"]`
- **THEN** the JWT is signed with the Auth Service's RSA private key
- **THEN** the JWT includes `iss`, `aud`, `iat`, and `exp` standard claims

#### Scenario: Login with invalid password
- **WHEN** a client sends `POST /auth/login` with a correct email but wrong password
- **THEN** the response returns 401 Unauthorized

#### Scenario: Login with nonexistent email
- **WHEN** a client sends `POST /auth/login` with an unregistered email
- **THEN** the response returns 401 Unauthorized (same message as wrong password, no user enumeration)

### Requirement: Auth Service exposes JWKS endpoint
The Auth Service SHALL expose `GET /.well-known/jwks` returning the RSA public key in JWKS format for JWT validation by consuming services.

#### Scenario: Consumer retrieves JWKS
- **WHEN** a consuming service sends `GET /.well-known/jwks`
- **THEN** the response returns 200 with a JWKS document containing the Auth Service's RSA public key

### Requirement: Auth Service manages role assignment
The Auth Service SHALL allow administrators to assign and remove roles from users via its API.

#### Scenario: Admin assigns role to user
- **WHEN** an authorized admin sends `POST /auth/users/{userId}/roles` with a valid role ID
- **THEN** a `UserRole` record is created linking the user to the specified role
- **THEN** the response returns 200

#### Scenario: Admin removes role from user
- **WHEN** an authorized admin sends `DELETE /auth/users/{userId}/roles/{roleId}`
- **THEN** the `UserRole` record is removed
- **THEN** the response returns 200

#### Scenario: Assign role to nonexistent user
- **WHEN** an authorized admin sends `POST /auth/users/{nonexistentId}/roles`
- **THEN** the response returns 404 Not Found

### Requirement: JWT expires after configured time
The Auth Service SHALL issue JWTs with a configurable expiration time (default: 60 minutes).

#### Scenario: Expired JWT rejected
- **WHEN** a consuming service receives a request with an expired JWT
- **THEN** the consuming service rejects the request with 401 Unauthorized

### Requirement: Role GUIDs are deterministic
The Auth Service SHALL seed roles with deterministic GUIDs matching the Permission Service: Admin=11111111-1111-1111-1111-111111111111, Viewer=22222222-2222-2222-2222-222222222222, Standard=33333333-3333-3333-3333-333333333333. Additionally, the Auth Service SHALL consume role lifecycle events from the Permission Service to keep its Role table in sync with any roles created, updated, or deleted at runtime.

#### Scenario: Seed roles exist on startup
- **WHEN** the Auth Service database is migrated
- **THEN** three roles exist with the deterministic GUIDs and names "Admin", "Viewer", "Standard"

#### Scenario: Auth receives role created event
- **WHEN** the Auth Service receives a `RoleCreatedEvent` for a new role GUID
- **THEN** a new Role record is created in the Auth database with the event's GUID and name
- **THEN** the role is available for user-role assignments

#### Scenario: Auth receives role deleted event
- **WHEN** the Auth Service receives a `RoleDeletedEvent` for an existing role GUID
- **THEN** the Role record is deleted from the Auth database
- **THEN** any UserRole records referencing that role are cascade-deleted

### Requirement: Auth Service exposes endpoint to list all users
The Auth Service SHALL expose `GET /auth/users` returning a list of all registered users with their assigned roles.

#### Scenario: Authorized user lists all users
- **WHEN** an authenticated client sends `GET /auth/users`
- **THEN** the response returns 200 with an array of user objects
- **AND** each user object contains `id`, `firstName`, `lastName`, `email`, and `roles` (array of role name strings)

#### Scenario: Empty user list
- **WHEN** an authenticated client sends `GET /auth/users` and no users exist
- **THEN** the response returns 200 with an empty array

#### Scenario: Unauthorized access to list users
- **WHEN** an unauthenticated client sends `GET /auth/users`
- **THEN** the response returns 401 Unauthorized

### Requirement: Auth Service exposes endpoint to get user by ID
The Auth Service SHALL expose `GET /auth/users/{userId}` returning a single user with their assigned roles.

#### Scenario: Authorized user fetches user by ID
- **WHEN** an authenticated client sends `GET /auth/users/{userId}` for an existing user
- **THEN** the response returns 200 with a user object
- **AND** the user object contains `id`, `firstName`, `lastName`, `email`, and `roles` (array of role name strings)

#### Scenario: User not found
- **WHEN** an authenticated client sends `GET /auth/users/{userId}` for a nonexistent user
- **THEN** the response returns 404 Not Found

#### Scenario: User with no roles
- **WHEN** an authenticated client sends `GET /auth/users/{userId}` for a user with no assigned roles
- **THEN** the response returns 200 with a user object
- **AND** the `roles` array is empty

#### Scenario: Unauthorized access to get user
- **WHEN** an unauthenticated client sends `GET /auth/users/{userId}`
- **THEN** the response returns 401 Unauthorized
