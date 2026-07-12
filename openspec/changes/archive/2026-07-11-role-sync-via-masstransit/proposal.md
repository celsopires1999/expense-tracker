## Why

The Permission Service and Auth Service each maintain their own `Role` table with identical seed data (Admin, Viewer, Standard) using deterministic GUIDs. There is no synchronization mechanism — when an admin creates, updates, or deletes a role in the Permission Service, the Auth Service knows nothing about it. This means Auth's role list diverges from Permission's at runtime, breaking JWT role claims and user-role assignments for any non-seeded role. We need Permission to publish role lifecycle events and Auth to consume them, keeping both role stores in sync.

## What Changes

- **BREAKING**: Permission Service publishes `RoleCreatedEvent`, `RoleUpdatedEvent`, `RoleDeletedEvent` via MassTransit when roles are created, updated, or deleted.
- **BREAKING**: Auth Service consumes role lifecycle events and upserts/deletes its local Role records accordingly.
- **BREAKING**: MassTransit transport changes from `InMemory` to `RabbitMQ` across all three contexts (Auth, Permission, Expense) to enable cross-process messaging.
- Add RabbitMQ as an infrastructure service in docker-compose.
- Add `IDomainEventsDispatcher` and `SaveChangesAsync` override to Permission.Infrastructure (matching Expense's existing pattern).
- Create shared event contracts (`RoleCreatedEvent`, `RoleUpdatedEvent`, `RoleDeletedEvent`) in SharedKernel.
- Switch all three DbContext MassTransit configurations from InMemory to RabbitMQ transport.

## Capabilities

### New Capabilities
- `role-sync`: Cross-context role synchronization via MassTransit events. Permission publishes role lifecycle events; Auth consumes them to maintain a consistent role store.

### Modified Capabilities
- `permission-management`: Role CRUD commands now raise domain events and publish them via MassTransit outbox. No API behavior changes — this is an internal side effect.
- `user-auth`: Auth Service now consumes role sync events to keep its Role table current. No API behavior changes — the Role table is updated automatically.

## Impact

- **Code**: Permission.Infrastructure gains domain events dispatcher + SaveChangesAsync override. Permission command handlers raise events. Auth.Infrastructure gains 3 MassTransit consumers. SharedKernel gains 3 event contracts. All 3 contexts change MassTransit DI from InMemory to RabbitMQ.
- **Infrastructure**: New RabbitMQ container in docker-compose. All three services need RabbitMQ connection configuration.
- **Database**: No schema changes — MassTransit outbox tables already exist. RabbitMQ is additive.
- **Dependencies**: `MassTransit.RabbitMQ` NuGet package added to all three Api/Infrastructure projects.
- **Configuration**: Each service's appsettings gains RabbitMQ connection string (`RabbitMQ:Host`, `RabbitMQ:Port`, etc.).
