## ADDED Requirements

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
