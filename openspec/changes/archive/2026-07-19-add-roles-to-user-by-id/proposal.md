## Why

The `GET /auth/users/{id}` endpoint returns user data without roles, while the `GET /auth/users` list endpoint includes roles for every user. This inconsistency means consumers must make two calls or use a separate `GET /auth/users/{id}/roles` endpoint to get a user's roles — something the list endpoint already provides in a single response.

## What Changes

- Add `roles` property (`string[]`) to the `UserResponse` DTO used by `GET /auth/users/{id}`
- Update `GetUserByIdQueryHandler` to query `UserRoles` + `Roles` and populate the roles array
- Add integration test coverage for roles in the single-user response

## Capabilities

### New Capabilities

_No new capabilities._

### Modified Capabilities

- `user-auth`: Add a new requirement/scenario for the `GET /auth/users/{id}` endpoint to include roles in its response, matching the behavior of the `GET /auth/users` list endpoint.

## Impact

- **Code**: `UserResponse.cs`, `GetUserByIdQueryHandler.cs`
- **Tests**: `Auth.IntegrationTests` — add scenario verifying roles in single-user response
- **API**: `GET /auth/users/{id}` response shape changes (adds `roles` field) — additive, non-breaking
