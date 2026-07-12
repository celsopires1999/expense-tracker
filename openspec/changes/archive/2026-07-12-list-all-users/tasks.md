## 1. Backend: List Users Endpoint

- [x] 1.1 Create `ListUsersResponse` DTO in Auth.Application (Id, FirstName, LastName, Email, string[] Roles)
- [x] 1.2 Create `GetAllUsersQuery` and `GetAllUsersQueryHandler` in Auth.Application — handler joins Users → UserRoles → Roles
- [x] 1.3 Create `ListUsers` endpoint in Auth.Api (`GET /auth/users`)
- [x] 1.4 Add integration tests: endpoint test (HTTP 200 + 401) and handler test (verifies roles are populated)
