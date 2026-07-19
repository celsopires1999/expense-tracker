## MODIFIED Requirements

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
