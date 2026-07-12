## Context

List all users in the system. Consuming services (e.g. Expense) need
visibility into registered users for administration and UI purposes.

## Goals / Non-Goals

**Goals:**
- Expose a read-only endpoint returning all users with their assigned roles
- Follow existing query patterns (GetUserById → GetAllUsers)

**Non-Goals:**
- Pagination / filtering (deferred to future change)
- Admin-only authorization (Auth.Api uses bare RequireAuthorization — any
  authenticated user can call this)
- User creation / deletion (already exists)

## Decisions

- Response includes roles via UserRoles → Roles join (same pattern as
  GetUserRolesQueryHandler)
- New response DTO (ListUsersResponse) rather than extending UserResponse,
  to avoid breaking existing GetUserById/GetByEmail handlers
- Bare .RequireAuthorization() — matches existing Auth.Api convention

