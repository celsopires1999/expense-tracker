## 1. DTO Update

- [x] 1.1 Add `string[] Roles` property to `UserResponse` in `src/Auth/Auth.Application/Users/GetById/UserResponse.cs`

## 2. Handler Update

- [x] 2.1 Update `GetUserByIdQueryHandler` to join `UserRoles` + `Roles` and project roles as an inline subquery in `src/Auth/Auth.Application/Users/GetById/GetUserByIdQueryHandler.cs`

## 3. Tests

- [x] 3.1 Add integration test verifying `GET /auth/users/{id}` returns roles in the response
- [x] 3.2 Add integration test verifying `GET /auth/users/{id}` returns empty `roles` array for user with no assigned roles
