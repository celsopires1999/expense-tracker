## ADDED Requirements

### Requirement: Standard role assignment at registration
The system SHALL assign the Standard role to every newly registered user automatically.

#### Scenario: New user receives Standard role
- **WHEN** a user completes registration via `POST /users/register`
- **THEN** the system creates a `UserRole` record linking the new user to the Standard role

#### Scenario: Standard user has gerenciamento de despesas
- **WHEN** a user with the Standard role sends a request to create, read, update, or delete an expense
- **THEN** the authorization handler SHALL permit the operation based on the `expenses:create`, `expenses:read`, `expenses:update`, and `expenses:delete` permissions

#### Scenario: Standard user can read reference data
- **WHEN** a user with the Standard role sends a GET request to categories, payment-methods, or tags endpoints
- **THEN** the authorization handler SHALL permit the operation based on `categories:read`, `payment-methods:read`, and `tags:read` permissions

#### Scenario: Standard user can access own profile
- **WHEN** a user with the Standard role sends a GET request to `/users/{ownUserId}`
- **THEN** the authorization handler SHALL permit the operation based on the `users:access` permission

### Requirement: Standard role permissions
The system SHALL define the Standard role with exactly the following permissions: `expenses:create`, `expenses:read`, `expenses:update`, `expenses:delete`, `categories:read`, `payment-methods:read`, `tags:read`, `users:access`.

#### Scenario: Permissions are seeded
- **WHEN** the database is initialized
- **THEN** the `role_permissions` table SHALL contain the eight permission records linked to the Standard role
