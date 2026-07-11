## ADDED Requirements

### Requirement: Auth handler-level integration tests
The Auth integration test project SHALL verify all CQRS handlers against a real PostgreSQL database with EF Core migrations applied.

#### Scenario: RegisterUserCommand creates user in database
- **WHEN** `RegisterUserCommandHandler` handles a valid `RegisterUserCommand`
- **THEN** a user record exists in the database with the provided email, first name, and last name
- **AND** the password is stored as a PBKDF2 hash (not plaintext)
- **AND** the "Standard" role is assigned to the user
- **AND** the returned GUID matches the created user's Id

#### Scenario: RegisterUserCommand rejects duplicate email
- **WHEN** `RegisterUserCommandHandler` handles a command with an email that already exists
- **THEN** the result is a Conflict error (`Users.EmailNotUnique`)

#### Scenario: RegisterUserCommand fails validation
- **WHEN** `RegisterUserCommandHandler` handles a command with empty email or password shorter than 8 characters
- **THEN** the result is a ValidationFailure error

#### Scenario: LoginUserCommand returns valid JWT
- **WHEN** `LoginUserCommandHandler` handles a valid login for an existing user
- **THEN** the result is a JWT string
- **AND** the JWT contains `sub`, `email`, and `role` claims
- **AND** the JWT is verifiable with the test RSA public key

#### Scenario: LoginUserCommand rejects wrong credentials
- **WHEN** `LoginUserCommandHandler` handles a login with unknown email or wrong password
- **THEN** the result is a `Users.NotFoundByEmail` error
- **AND** the error is the same for both cases (security: hide which part failed)

#### Scenario: GetUserByIdQuery returns user without password hash
- **WHEN** `GetUserByIdQueryHandler` handles a query for an existing user
- **THEN** the result contains Id, FirstName, LastName, and Email
- **AND** the result does NOT contain PasswordHash

#### Scenario: GetUserRolesQuery returns role names
- **WHEN** `GetUserRolesQueryHandler` handles a query for a user with assigned roles
- **THEN** the result contains the role name strings

#### Scenario: AssignRoleCommand is idempotent
- **WHEN** `AssignRoleCommandHandler` assigns a role that is already assigned
- **THEN** the result is Success (no error, no duplicate record)

#### Scenario: RemoveRoleCommand is idempotent
- **WHEN** `RemoveRoleCommandHandler` removes a role that is not assigned
- **THEN** the result is Success (no error)

### Requirement: Auth endpoint-level integration tests
The Auth integration test project SHALL verify all HTTP endpoints via `WebApplicationFactory<Program>` with real PostgreSQL.

#### Scenario: POST /auth/register returns 201 with Guid
- **WHEN** a valid registration request is sent to `POST /auth/register`
- **THEN** the response is 201 Created
- **AND** the response body contains the new user's GUID

#### Scenario: POST /auth/register returns 400 for invalid input
- **WHEN** a request with invalid email or short password is sent to `POST /auth/register`
- **THEN** the response is 400 Bad Request with validation errors

#### Scenario: POST /auth/register returns 409 for duplicate email
- **WHEN** a request with an existing email is sent to `POST /auth/register`
- **THEN** the response is 409 Conflict

#### Scenario: POST /auth/login returns 200 with token
- **WHEN** valid credentials are sent to `POST /auth/login`
- **THEN** the response is 200 OK
- **AND** the response body contains `{ "Token": "eyJ..." }`

#### Scenario: POST /auth/login returns 404 for wrong credentials
- **WHEN** invalid credentials are sent to `POST /auth/login`
- **THEN** the response is 404 Not Found

#### Scenario: GET /auth/users/{id} requires authentication
- **WHEN** a request without a JWT token is sent to `GET /auth/users/{id}`
- **THEN** the response is 401 Unauthorized

#### Scenario: GET /auth/users/{id} returns user with valid token
- **WHEN** a request with a valid JWT token is sent to `GET /auth/users/{id}`
- **THEN** the response is 200 OK with `UserResponse`

#### Scenario: GET /.well-known/jwks returns valid JWKS
- **WHEN** a request is sent to `GET /.well-known/jwks`
- **THEN** the response is 200 OK
- **AND** the response body contains a JWKS JSON with an RSA key
- **AND** the key can verify tokens issued by the test JWT configuration
