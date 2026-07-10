## 1. Foundation — Project Structure & Shared Kernel

- [x] 1.1 Restructure `src/` into `src/Auth/`, `src/Permission/`, `src/ExpenseTracker/`, each with Domain/Application/Infrastructure/Api projects
- [x] 1.2 Update `ExpenseTracker.slnx` to reference all new projects
- [x] 1.3 Copy `SharedKernel` as a shared project reference consumed by all three services
- [x] 1.4 Update `Directory.Build.props` and `Directory.Packages.props` for multi-service build
- [x] 1.5 Copy and adapt `.editorconfig` for new projects

## 2. Auth Service — Domain & Application

- [x] 2.1 Create `Auth.Domain` with `User` (id, email, firstName, lastName, passwordHash), `UserRole` (userId, roleId), and `Role` (id, name) entities
- [x] 2.2 Create `Auth.Application` with `RegisterUserCommand`/`Handler`, `LoginUserCommand`/`Handler`, `AssignRoleCommand`/`Handler`, `RemoveRoleCommand`/`Handler`, `GetUserRolesQuery`/`Handler`
- [x] 2.3 Add FluentValidation validators for auth commands
- [x] 2.4 Create `ITokenProvider` (RSA JWT) and `IPasswordHasher` abstractions in `Auth.Application`
- [x] 2.5 Add Scrutor DI registration with logging + validation decorators in `Auth.Application`

## 3. Auth Service — Infrastructure & API

- [x] 3.1 Create `Auth.Infrastructure` with EF Core DbContext for Users, UserRoles, Roles tables
- [x] 3.2 Add `RoleConfiguration` seeding Admin/Viewer/Standard with deterministic GUIDs
- [x] 3.3 Add `UserConfiguration` and `UserRoleConfiguration` EF configurations
- [x] 3.4 Implement `TokenProvider` with RSA asymmetric JWT signing (generate key pair, sign with private key)
- [x] 3.5 Implement `PasswordHasher` (PBKDF2-SHA512, same as current)
- [x] 3.6 Add JWKS endpoint configuration (`GET /.well-known/jwks`)
- [ ] 3.7 Implement MassTransit outbox with PostgreSQL — publish `UserCreated` event on registration
- [x] 3.8 Wire DI in `Auth.Infrastructure.DependencyInjection`: auth, EF, MassTransit, RSA key provider
- [x] 3.9 Create `Auth.Api` project with minimal API endpoints: `POST /auth/register`, `POST /auth/login`, `POST /auth/users/{id}/roles`, `DELETE /auth/users/{id}/roles/{roleId}`, `GET /.well-known/jwks`
- [x] 3.10 Register endpoints via Scrutor scanning (same `IEndpoint` pattern)
- [x] 3.11 Configure Swagger for Auth API

## 4. Permission Service — Domain & Application

- [x] 4.1 Create `Permission.Domain` with `Role` (id, name) and `RolePermission` (roleId, permission) entities
- [x] 4.2 Create `Permission.Application` with `CreateRoleCommand`/`Handler`, `UpdateRolePermissionsCommand`/`Handler`, `DeleteRoleCommand`/`Handler`, `ResolvePermissionsQuery`/`Handler`, `GetRolesQuery`/`Handler`
- [x] 4.3 Add FluentValidation validators for permission commands
- [x] 4.4 Add Scrutor DI registration with logging + validation decorators

## 5. Permission Service — Infrastructure & API

- [x] 5.1 Create `Permission.Infrastructure` with EF Core DbContext for Roles, RolePermissions tables
- [x] 5.2 Add `RoleConfiguration` and `RolePermissionConfiguration` with seed data (Admin all perms, Viewer read-only, Standard CRUD own)
- [x] 5.3 Wire DI in `Permission.Infrastructure.DependencyInjection`: EF, MassTransit
- [x] 5.4 Create `Permission.Api` project with minimal API endpoints: `POST /permissions/roles`, `GET /permissions/roles`, `GET /permissions/roles/{id}`, `PUT /permissions/roles/{id}/permissions`, `POST /permissions/resolve`
- [x] 5.5 Register endpoints via Scrutor scanning
- [x] 5.6 Configure Swagger for Permission API

## 6. Expense Tracker — Remove Auth Entities & Database Tables

- [ ] 6.1 Remove `User`, `UserRole`, `Role`, `RolePermission` entities from `Domain`
- [ ] 6.2 Remove `UserErrors`, `UserRegisteredDomainEvent`, and any auth-related domain events
- [ ] 6.3 Drop `Users`, `UserRoles`, `Roles`, `RolePermissions` tables from Expense Tracker DbContext
- [ ] 6.4 Create migration removing auth tables from Expense Tracker database

## 7. Expense Tracker — Remove Auth Commands & Infrastructure

- [ ] 7.1 Remove `RegisterUserCommand`/`Handler`, `LoginUserCommand`/`Handler`, `GetUserByIdQuery`/`Handler`, `GetUserByEmailQuery`/`Handler` from `Application`
- [ ] 7.2 Remove `IUserContext`, `ITokenProvider`, `IPasswordHasher` abstractions from `Application.Abstractions`
- [ ] 7.3 Remove `TokenProvider`, `PasswordHasher`, `ClaimsPrincipalExtensions`, `UserContext`, `UserContextUnavailableException` from `Infrastructure`
- [ ] 7.4 Remove `PermissionAuthorizationHandler`, `PermissionAuthorizationPolicyProvider`, `PermissionProvider`, `PermissionRequirement`, `HasPermissionAttribute` from `Infrastructure`
- [ ] 7.5 Remove `UserConfiguration`, `RoleConfiguration`, `UserRoleConfiguration`, `RolePermissionConfiguration` from `Infrastructure`
- [ ] 7.6 Remove Scrutor decoration for auth services from `Infrastructure.DependencyInjection`

## 8. Expense Tracker — Add Permission Client & Cache

- [ ] 8.1 Create `IPermissionServiceClient` abstraction in `Application` with `ResolvePermissionsAsync(string[] roles): HashSet<string>`
- [ ] 8.2 Implement `PermissionServiceClient` in `Infrastructure` using `HttpClient` calling `POST /permissions/resolve` on Permission Service
- [ ] 8.3 Add `PermissionCache` with `IMemoryCache` and configurable TTL (default 5 min), keyed by `perms:{userId}`
- [ ] 8.4 Create new `PermissionAuthorizationHandler` that extracts roles from JWT, checks cache, calls Permission Service on miss
- [ ] 8.5 Configure `PermissionAuthorizationPolicyProvider` (unchanged pattern — still dynamic policy from permission strings)
- [ ] 8.6 Register all new services in `Infrastructure.DependencyInjection`

## 9. Expense Tracker — Update JWT Validation

- [ ] 9.1 Replace symmetric JWT validation with RSA public key validation via JWKS endpoint
- [ ] 9.2 Configure `AddAuthentication().AddJwtBearer()` with `JwtSecurityKeyResolver` that fetches keys from Auth Service JWKS
- [ ] 9.3 Update `ClaimsPrincipalExtensions` to extract `roles` claim instead of `perm_version`
- [ ] 9.4 Update `UserContext` if needed (UserId still from `sub` claim)

## 10. Expense Tracker — Update Endpoints

- [ ] 10.1 Remove `Endpoints/Users/Login.cs`, `Register.cs`, `GetById.cs` (moved to Auth Service)
- [ ] 10.2 Keep `HasPermission()` extension method — it still calls `RequireAuthorization(permissionString)`
- [ ] 10.3 Verify all remaining endpoints use `.HasPermission()` with correct permission strings
- [ ] 10.4 Remove permission constant files from `Endpoints/Users/Permissions.cs` — move to Permission Service or keep as constants in Expense Tracker
- [ ] 10.5 Update `Program.cs`: remove auth-related middleware and service registrations

## 11. Integration — MassTransit Configuration

- [ ] 11.1 Add `MassTransit.EntityFrameworkCore` NuGet package to Auth, Permission, Expense Tracker Api projects
- [ ] 11.2 Configure MassTransit with PostgreSQL transport in each service's DI
- [ ] 11.3 Add outbox tables to each service's DbContext (via MassTransit EF migrations)
- [ ] 11.4 Configure `UserCreated` event consumption in Permission Service (optional — currently no consumer needed)
- [ ] 11.5 Configure `UserCreated` event consumption in Expense Tracker (optional — currently no consumer needed)

## 12. Infrastructure — Docker Compose & DevContainer

- [ ] 12.1 Add three service containers to `.devcontainer/docker-compose.yml`: `auth-api`, `permission-api`, `expense-api`
- [ ] 12.2 Add three PostgreSQL databases (or three containers) — `auth-db`, `permission-db`, `expense-db`
- [ ] 12.3 Configure service-to-service networking (internal network for HTTP + MassTransit)
- [ ] 12.4 Update `justfile` with recipes for each service: `build-auth`, `build-perm`, `build-expense`, `test-auth`, etc.
- [ ] 12.5 Update devcontainer `postCreateCommand` if needed
- [ ] 12.6 Configure service-to-service authentication (API keys in environment or client credentials JWT)

## 13. Verification — Tests & Build

- [ ] 13.1 Update `ArchitectureTests` to verify new layer dependencies across all three services
- [ ] 13.2 Write unit tests for Auth Service commands/queries
- [ ] 13.3 Write unit tests for Permission Service commands/queries
- [ ] 13.4 Write unit tests for Expense Tracker permission client and cache
- [ ] 13.5 Update or remove tests for removed auth commands from Expense Tracker
- [ ] 13.6 Verify `dotnet build` passes for entire solution
- [ ] 13.7 Verify `dotnet test` passes for all test projects
- [ ] 13.8 Run `just format` and fix any code style issues

## 14. Cleanup — Remove Legacy Code

- [ ] 14.1 Remove `IUserContext`, `ITokenProvider`, `IPasswordHasher` from `Application/Abstractions/Authentication/`
- [ ] 14.2 Remove `src/Infrastructure/Authentication/` entire directory
- [ ] 14.3 Remove `src/Infrastructure/Authorization/` entire directory
- [ ] 14.4 Remove `src/Infrastructure/Roles/` entire directory
- [ ] 14.5 Remove `src/Infrastructure/Users/UserConfiguration.cs`
- [ ] 14.6 Remove `src/Domain/Users/User.cs`, `UserErrors.cs`, `UserRegisteredDomainEvent.cs`
- [ ] 14.7 Remove `src/Domain/Roles/` entire directory
- [ ] 14.8 Remove `src/Web.Api/Endpoints/Users/` entire directory
- [ ] 14.9 Remove `src/Web.Api/Endpoints/Expenses/Permissions.cs`, `Categories/Permissions.cs`, etc. (kept as constants in Expense Tracker if needed)
- [ ] 14.10 Update `Application.DependencyInjection` — remove auth-related registrations
