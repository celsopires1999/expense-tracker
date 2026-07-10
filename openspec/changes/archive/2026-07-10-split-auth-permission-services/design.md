## Context

The Expense Tracker is a DDD + CQRS monolith with 5 projects (SharedKernel, Domain, Application, Infrastructure, Web.Api). Authentication (JWT, password hashing, user management) and authorization (roles, permissions, policy-based checking) are deeply embedded in the Infrastructure and Domain layers, sharing a single PostgreSQL database with the business entities (Expenses, Categories, etc.).

The system works but cannot be reused — other applications would need to duplicate the auth and permission logic. The twin goals are: (a) extract two standalone, centralized services (Auth, Permission) that multiple apps can consume, and (b) keep the Expense Tracker focused on its domain.

## Goals / Non-Goals

**Goals:**
- Decompose monolith into three independently deployable services: Auth, Permission, Expense Tracker
- Auth and Permission services are reusable — consumed by multiple applications (SSO-style)
- Each service owns its own PostgreSQL database
- RSA asymmetric JWT signing with JWKS endpoint for public key distribution
- TTL-based permission caching (no `perm_version` claim)
- MassTransit with PostgreSQL transport for async events (no RabbitMQ)
- Namespaced permission strings (`expense:create`) for cross-app compatibility
- Deterministic role GUIDs seeded in both Auth and Permission services
- All development stays inside the existing devcontainer

**Non-Goals:**
- Not building a full multi-tenant system (organizations/workspaces)
- No UI changes (the services expose APIs only)
- No changes to Expense Tracker domain logic (expenses, categories, payment methods, tags)
- No changes to SharedKernel abstractions (Entity, Result, Error, IDomainEvent, IDateTimeProvider)
- No migration of existing production data (this is speculative/early-stage)

## Decisions

### 1. JWT signing: RSA asymmetric over HMAC symmetric

**Decision**: Auth Service signs JWTs with RSA private key. All consuming services validate using the public key served at `/.well-known/jwks`.

**Rationale**: HMAC requires sharing the symmetric secret with every consumer — any compromised consumer leaks the key. RSA allows the private key to live only in the Auth Service; consumers only need the (non-secret) public key. This is the standard pattern for microservice architectures.

**Alternatives considered:**
- *HMAC (current)*: Simple but insecure for multi-service — any service can forge tokens.
- *ECDSA*: Equivalent security to RSA with smaller keys; viable alternative. RSA chosen for wider library support and simpler key generation.
- *Shared secret per service*: Increases operational complexity — each consumer gets a different secret, Auth needs to track which secret to use.

### 2. Permission resolution: HTTP API call over embedding permissions in JWT

**Decision**: Permission Service exposes `POST /permissions/resolve` that accepts `{ roles: ["Standard"] }` and returns `{ permissions: ["expense:create", ...] }`. Expense Tracker calls this endpoint on cache miss.

**Rationale**: Keeping permissions out of the JWT keeps tokens small and avoids the need to re-issue tokens when permissions change. The TTL-based cache (1-5 min) absorbs the latency of repeated calls. The roles (not permissions) in the JWT allow the Expense Tracker to know the user's role membership without calling the Auth Service on every request.

**Alternatives considered:**
- *Permissions in JWT*: Token grows with permission count; can't revoke without re-issuance.
- *Centralized authorization gateway*: Adds a new infrastructure component and a synchronous hop on every request.
- *Direct DB access by Expense Tracker*: Violates service boundaries — Expense Tracker would need access to the Permission database.

### 3. Cache invalidation: TTL-only over versioned cache key

**Decision**: Expense Tracker caches resolved permissions with a configurable TTL (default: 5 minutes). No `perm_version` claim, no versioned cache keys, no invalidation events.

**Rationale**: Simplicity. The versioned approach (`perm_version`) required cross-service notifications (Permission → Auth → increment version) and stored the version on the User entity — which now lives in the Auth database, not the Permission database. The TTL window (5 minutes) is acceptable for an internal tool where permission changes are infrequent and not time-critical.

**Alternatives considered:**
- *perm_version (current)*: Clean but requires Auth to know about permission changes — coupling across service boundaries.
- *Global permission version*: Any permission change invalidates all user caches — unnecessary cache churn.
- *Webhook invalidation*: Permission Service calls Expense Tracker endpoints to flush specific entries — more complex, adds failure modes.

### 4. Message transport: MassTransit + PostgreSQL over RabbitMQ / Kafka

**Decision**: MassTransit with PostgreSQL transport. Each service's database hosts its own MassTransit outbox/inbox/queue tables.

**Rationale**: Zero additional infrastructure — PostgreSQL is already required. MassTransit's EF Core outbox pattern guarantees exactly-once delivery within the same transaction as domain events. Suitable for the low-volume, non-realtime events in this system (UserCreated, RolePermissionsChanged if needed later).

**Alternatives considered:**
- *RabbitMQ*: Requires running and maintaining a RabbitMQ cluster. Overkill for current event volume.
- *Kafka*: Designed for high-throughput event streaming — wrong tool for occasional domain events.
- *Azure Service Bus / Amazon SQS*: Cloud-specific, introduces vendor lock-in.

### 5. Service boundaries: Auth owns UserRole, Permission owns RolePermission

**Decision**: Auth Service owns Users + UserRoles. Permission Service owns Roles + RolePermissions. Resolution requires the consumer (Expense Tracker) to pass the user's roles (from JWT) to the Permission Service.

**Rationale**: User-to-role assignment is a user management concern (Auth). Role-to-permission mapping is an authorization concern (Permission). The JWT carries roles as claims, enabling the Expense Tracker to bridge the two without either service needing to reference the other's database.

**Alternatives considered:**
- *Permission owns UserRole*: Permission would own a Users table reference — cross-database referential integrity concerns.
- *Both own a UserRole table*: Data duplication and sync complexity.
- *Auth embeds permissions in JWT at login*: Auth would need to call Permission Service synchronously at login, adding latency and a hard dependency to the login flow.

### 6. Role GUIDs: Deterministic seeding over dynamic discovery

**Decision**: Both Auth and Permission services seed roles with deterministic GUIDs (same as current: Admin=1111..., Viewer=2222..., Standard=3333...). No runtime discovery API needed.

**Rationale**: Simplicity and zero coupling at startup. The three roles are well-known, rarely change, and their GUIDs are already established. Auth assigns roles by known GUID; Permission maps known GUIDs to permissions.

**Alternatives considered:**
- *Auth calls Permission to discover roles*: Adds startup dependency and latency.
- *Roles as strings only (no GUIDs)*: Works but loses the referential clarity of a stable identifier.
- *Configuration-based role mapping*: Role GUIDs in appsettings — same effect, more ceremony.

### 7. Project structure: Monorepo with shared project over NuGet

**Decision**: All three services live in the same solution (`ExpenseTracker.slnx`), each as a top-level `src/<Service>/` folder with its own Domain/Application/Infrastructure/Api projects. SharedKernel remains a shared project reference.

**Rationale**: Simpler development workflow — single `docker compose up`, shared DevContainer, unified build/test. The services can be extracted to separate repos later if needed. SharedKernel as a project reference (not NuGet) avoids package versioning overhead during development.

**Alternatives considered:**
- *Separate repositories*: Independent CI/CD, but adds friction for development and cross-service refactoring.
- *SharedKernel as NuGet*: Versioning discipline needed; premature for early-stage decomposition.

## Risks / Trade-offs

- **TTL-based cache**: Permission changes take up to 5 minutes to propagate. Insufficient for scenarios requiring immediate revocation. Mitigation: support admin-triggered cache flush endpoint on Expense Tracker for emergency revocation.
- **HTTP coupling between Expense Tracker and Permission Service**: If Permission Service is down, new cache entries can't be resolved (existing cache continues working). Mitigation: circuit breaker pattern, stale-if-error cache strategy.
- **Three databases**: Increased operational complexity compared to single database. Mitigation: all PostgreSQL, managed via same Docker Compose, unified migration tooling.
- **Service-to-service auth not yet designed**: Expense Tracker calling Permission Service needs authentication. Mitigation: deferred to implementation phase — likely API key or client credentials JWT.
- **Existing data migration**: No production data exists yet, but migration scripts will be needed for the eventual transition. Mitigation: staging environment validation before cutover.
- **JWT audience/issuer management**: Each consuming service needs configured `ValidIssuer` and `ValidAudience`. Misconfiguration causes auth failures. Mitigation: centralize in SharedKernel or configuration conventions.
