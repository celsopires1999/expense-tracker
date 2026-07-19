## Context

The Auth Service exposes two user-listing endpoints:

- `GET /auth/users` — returns `ListUsersResponse[]` with `id`, `firstName`, `lastName`, `email`, and `roles`
- `GET /auth/users/{id}` — returns `UserResponse` with `id`, `firstName`, `lastName`, `email` (no roles)

The list endpoint uses a two-step strategy: project users, then batch-fetch roles via a single `UserRoles JOIN Roles` query grouped by `UserId`. The single-user endpoint (`GetUserByIdQueryHandler`) runs a single `SELECT` against `Users` with no join to roles.

There is also an existing standalone `GetUserRolesQuery` that queries `UserRoles JOIN Roles` for a single user and returns `string[]` — currently not consumed by the `GetById` endpoint.

## Goals / Non-Goals

**Goals:**
- Add `roles` (`string[]`) to the `UserResponse` DTO so `GET /auth/users/{id}` returns roles
- Keep the handler efficient (single query with join, not N+1)
- Add integration test coverage

**Non-Goals:**
- Refactoring `ListUsersResponse` and `UserResponse` into a shared DTO (out of scope)
- Changing the list endpoint behavior
- Adding pagination or filtering

## Decisions

### Join strategy in GetUserByIdQueryHandler

**Decision**: Extend the existing LINQ query to join `UserRoles` + `Roles` inline, projecting `roles` as a subquery in the same SELECT.

**Rationale**: For a single user, an inline subquery (`UserRoles.Where(ur => ur.UserId == u.Id).Join(Roles, ...)`) keeps it to one round-trip and mirrors the semantics of the list handler without the batch complexity. The existing `GetUserRolesQuery` could be called separately, but that would add a second DB round-trip for no benefit.

**Alternative considered**: Call `GetUserRolesQuery` from the handler — rejected because it adds an extra query for data that can be fetched in a single round-trip.

## Risks / Trade-offs

- **Additive API change**: Adding `roles` to the response is non-breaking. Consumers that don't expect it will simply ignore the new field.
- **No performance concern**: A single-user join with `UserRoles` + `Roles` on indexed foreign keys is negligible overhead.
