## Context

The system has three bounded contexts (Auth, Permission, Expense), each with its own PostgreSQL database and MassTransit EF outbox configured. Currently, all three use `InMemory` transport — meaning the outbox exists but no cross-process messaging is possible. Auth and Permission both maintain a `Role` table with identical seed data (same GUIDs), but there is no runtime synchronization. When an admin creates a new role via Permission.Api, Auth.Api is unaware of it, breaking JWT role claims for users assigned to that role.

The Permission context has no domain events dispatcher — its `SaveChangesAsync` is the default EF implementation. The Expense context already has a working `IDomainEventsDispatcher` pattern with `SaveChangesAsync` override that can be reused.

## Goals / Non-Goals

**Goals:**
- Permission Service publishes role lifecycle events (created, updated, deleted) via MassTransit
- Auth Service consumes these events and keeps its Role table in sync
- Switch MassTransit transport from InMemory to RabbitMQ across all three contexts
- Use the existing EF outbox pattern for reliable, transactional message publishing
- Shared event contracts in SharedKernel so both Publisher and Consumer reference the same types

**Non-Goals:**
- Syncing role-permission assignments to Auth (Auth only needs role existence, not permissions)
- Syncing Auth-created roles back to Permission (Permission is the source of truth for roles)
- Real-time propagation guarantees (eventual consistency within outbox poll interval is acceptable)
- Changes to the Expense context's role consumption (it uses HTTP to Permission, not role storage)
- Adding new API endpoints (this is purely internal event-driven behavior)

## Decisions

### 1. Transport: RabbitMQ over PostgreSQL transport

**Decision**: Add RabbitMQ to docker-compose and use `MassTransit.RabbitMQ` transport for all three contexts.

**Rationale**: RabbitMQ is the standard MassTransit transport with first-class support. The PostgreSQL transport works but is polling-based and less mature. RabbitMQ gives better throughput, native push-based delivery, and proper exchange/queue topology. The infrastructure cost is minimal (one container).

**Alternatives considered:**
- *PostgreSQL transport*: No new infra, but polling-based (latency = poll interval). MassTransit's PG transport is less battle-tested. More complex config with queue tables.
- *InMemory (current)*: Only works within a single process. Cannot cross service boundaries.
- *Azure Service Bus / Amazon SQS*: Cloud-specific, adds vendor lock-in. Not suitable for local dev.

### 2. Event publishing: Domain events → MassTransit publisher

**Decision**: Permission command handlers raise domain events on the entity (`Role.Raise()`). The `SaveChangesAsync` override extracts events and dispatches them via `IDomainEventsDispatcher`. A domain event handler publishes the corresponding MassTransit event via `IPublishEndpoint`.

**Rationale**: Reuses the proven Expense context pattern. Domain events stay in the domain layer (clean architecture). The MassTransit publish happens inside the domain event handler, which runs after `SaveChangesAsync` succeeds — so the outbox captures the message in the same transaction.

**Alternatives considered:**
- *Publish directly in command handlers*: Simpler but couples application layer to MassTransit. Violates the existing pattern.
- *MediatR notification → MassTransit bridge*: Adds unnecessary abstraction. Domain events + dispatcher is already the pattern.
- *Transactional outbox only (no domain events)*: Bypasses the domain event abstraction. Works but breaks the established pattern.

### 3. Shared contracts in SharedKernel

**Decision**: Define `RoleCreatedEvent`, `RoleUpdatedEvent`, `RoleDeletedEvent` records in SharedKernel. Both Permission (publisher) and Auth (consumer) reference SharedKernel.

**Rationale**: SharedKernel is already a project reference in all three contexts. Putting contracts there avoids a separate messaging contracts project. The events are simple data records — no domain logic.

**Alternatives considered:**
- *Separate messaging contracts project*: Adds a project for 3 simple records. Over-engineering.
- *Duplicate contracts in each context*: Violates DRY. Risk of schema drift.
- *Contract via message attributes only*: MassTransit can match by message type string, but loses compile-time safety.

### 4. Consumer pattern: Upsert with natural idempotency

**Decision**: Auth consumers use role GUID as the natural idempotency key. `RoleCreatedConsumer` does an upsert (check if exists → create or update). `RoleDeletedConsumer` does a conditional delete.

**Rationale**: The outbox guarantees at-least-once delivery. Using the GUID as the key means duplicate messages are safe — the second `RoleCreatedEvent` for the same GUID is a no-op. MassTransit's inbox deduplication adds another safety layer.

**Alternatives considered:**
- *Inbox-only idempotency*: Relies on MassTransit inbox state. Works but the consumer should still be idempotent as a defense-in-depth measure.
- *Optimistic concurrency with retry*: More complex, unnecessary for simple upserts.

### 5. Auth consumer: Create missing roles silently

**Decision**: When Auth receives a `RoleCreatedEvent` for a role that already exists (e.g., seeded roles), it updates the name and continues. No error thrown.

**Rationale**: The three seeded roles (Admin, Viewer, Standard) exist in both databases with the same GUIDs. If Permission re-creates one (or the event replays), Auth should gracefully handle it rather than failing. The seeded data may diverge in name — the event is the source of truth.

**Alternatives considered:**
- *Skip if exists*: Simpler but ignores name updates from Permission. If Permission renames "Standard" to "User", Auth wouldn't reflect it.
- *Throw on conflict*: Breaks the consumer on replay or seed data overlap.

### 6. Expense context: Switch to RabbitMQ (no functional change)

**Decision**: Switch Expense.Infrastructure MassTransit from InMemory to RabbitMQ for consistency, even though it has no consumers or publishers yet.

**Rationale**: Keeps all three contexts on the same transport. When Expense eventually needs messaging (e.g., consuming Auth or Permission events), the transport is already configured. No additional work now.

**Alternatives considered:**
- *Leave Expense on InMemory*: Saves config but creates inconsistency. Migration later is more disruptive.

## Risks / Trade-offs

- **RabbitMQ as new infrastructure dependency**: Adds one more service to docker-compose. Mitigation: lightweight container, well-understood, optional for local dev (InMemory fallback possible).
- **Eventual consistency**: Role changes propagate within the outbox poll interval (1 second default). There is a window where Auth's Role table is stale. Mitigation: acceptable for internal tool; roles are rarely created at runtime.
- **At-least-once delivery**: Duplicate events possible. Mitigation: idempotent consumers using GUID as natural key + MassTransit inbox deduplication.
- **Seed data overlap**: Both Auth and Permission seed the same 3 roles. On first event receipt, Auth may update seeded roles. Mitigation: by design — event data is the source of truth.
- **SharedKernel coupling**: Event contracts in SharedKernel means all three projects reference them. Mitigation: contracts are simple data records with no dependencies, so the coupling is minimal and appropriate.
