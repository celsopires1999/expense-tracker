## Why

The current monolithic Web.Api couples authentication, authorization, and expense tracking into a single deployable unit. Auth and Permission logic cannot be reused by other applications, and the single database and shared JWT secret create security and scalability constraints. Splitting into three microservices enables the Auth and Permission services to operate as centralized, reusable infrastructure consumed by multiple applications (SSO-like), while the Expense Tracker focuses purely on business logic.

## What Changes

- **BREAKING**: Extract `Auth Service` — new standalone service managing Users and UserRoles. Handles login, register, JWT issuance with RSA asymmetric signing. Exposes `/.well-known/jwks` for public key distribution.
- **BREAKING**: Extract `Permission Service` — new standalone service managing Roles and RolePermissions. Provides resolution API (`POST /permissions/resolve`) to map role names to permission sets.
- **BREAKING**: Remove auth-related entities, permissions seed data, JWT infrastructure from current Web.Api. Expense Tracker no longer has Users, Roles, UserRoles, RolePermissions tables.
- **BREAKING**: Replace HMAC-symmetric JWT signing with RSA/ECDSA asymmetric signing. JWT claims change: drops `perm_version`, adds `roles` array.
- **BREAKING**: Replace versioned permission cache (`perm_version`-based) with TTL-based cache (1-5 minutes). Permission changes propagate within TTL window.
- **BREAKING**: Move `PermissionProvider` / `PermissionAuthorizationHandler` / `PermissionAuthorizationPolicyProvider` out of Infrastructure. Expense Tracker calls Permission Service via HTTP on cache miss.
- Replace direct permission DB queries with inter-service HTTP calls.
- Add MassTransit with PostgreSQL transport for asynchronous event communication (e.g., `UserCreated` events).
- Introduce SharedKernel as NuGet package (or shared project reference) consumed by all three services.
- Each service owns its own PostgreSQL database. Three databases total.

## Capabilities

### New Capabilities
- `user-auth`: User registration, login, JWT token issuance (RSA asymmetric), JWKS endpoint, UserRole assignment. Centralized SSO service.
- `permission-management`: Role definitions, role-to-permission mapping, permission resolution API. Namespaced permission strings for multi-app support.

### Modified Capabilities
- `permissions`: Requirements change from `perm_version`-based cache to TTL-based cache. Permission resolution moves from local DB query to HTTP call to Permission Service. JWT no longer carries permission version, roles are explicit claims instead.

## Impact

- **Code**: Current `src/` split into three service directories (or repositories). Auth-related Domain entities (User, UserRole) move to Auth service. Permission entities (Role, RolePermission) move to Permission service. Expense Tracker retains Expense, Category, PaymentMethod, Tag.
- **Infrastructure**: Three PostgreSQL databases, MassTransit + PostgreSQL transport (no RabbitMQ), RSA key pair for JWT.
- **CI/CD**: Three independent pipelines (or one matrix), each service builds and deploys separately.
- **Configuration**: JWT public key distribution via JWKS endpoint. Service-to-service auth (API keys or client credentials).
- **Development**: Devcontainer updates to run three services. Docker Compose changes for three API containers.
