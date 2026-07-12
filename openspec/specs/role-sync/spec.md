# Role Sync

## Purpose
Define cross-context synchronization of role lifecycle events between Permission Service and Auth Service via MassTransit/RabbitMQ.

## Requirements

### Requirement: Permission Service publishes role lifecycle events
The Permission Service SHALL publish a MassTransit event whenever a role is created, updated, or deleted. Events SHALL be persisted via the EF outbox pattern before delivery.

#### Scenario: Role created event published
- **WHEN** an authorized admin creates a new role via `POST /permissions/roles`
- **THEN** a `RoleCreatedEvent` containing the role's GUID, name, and timestamp is published to the MassTransit bus
- **THEN** the event is persisted in the Permission database outbox table in the same transaction as the role creation

#### Scenario: Role updated event published
- **WHEN** an authorized admin updates a role's name or permissions via `PUT /permissions/roles/{roleId}`
- **THEN** a `RoleUpdatedEvent` containing the role's GUID, new name, and timestamp is published to the MassTransit bus
- **THEN** the event is persisted in the Permission database outbox table in the same transaction

#### Scenario: Role deleted event published
- **WHEN** an authorized admin deletes a role via `DELETE /permissions/roles/{roleId}`
- **THEN** a `RoleDeletedEvent` containing the role's GUID and timestamp is published to the MassTransit bus
- **THEN** the event is persisted in the Permission database outbox table in the same transaction

### Requirement: Auth Service consumes role lifecycle events
The Auth Service SHALL consume `RoleCreatedEvent`, `RoleUpdatedEvent`, and `RoleDeletedEvent` from the MassTransit bus and maintain its local Role table in sync.

#### Scenario: Auth receives role created event
- **WHEN** the Auth Service receives a `RoleCreatedEvent` for a role GUID that does not exist in its Role table
- **THEN** a new Role record is created in the Auth database with the event's GUID and name

#### Scenario: Auth receives role created event for existing role
- **WHEN** the Auth Service receives a `RoleCreatedEvent` for a role GUID that already exists in its Role table
- **THEN** the existing Role record's name is updated to match the event
- **THEN** no error is thrown

#### Scenario: Auth receives role updated event
- **WHEN** the Auth Service receives a `RoleUpdatedEvent` for a role GUID that exists in its Role table
- **THEN** the Role record's name is updated to match the event

#### Scenario: Auth receives role updated event for nonexistent role
- **WHEN** the Auth Service receives a `RoleUpdatedEvent` for a role GUID that does not exist in its Role table
- **THEN** a new Role record is created with the event's GUID and name

#### Scenario: Auth receives role deleted event
- **WHEN** the Auth Service receives a `RoleDeletedEvent` for a role GUID that exists in its Role table
- **THEN** the Role record is deleted from the Auth database
- **THEN** any associated UserRole records for that role are cascade-deleted

#### Scenario: Auth receives role deleted event for nonexistent role
- **WHEN** the Auth Service receives a `RoleDeletedEvent` for a role GUID that does not exist in its Role table
- **THEN** no error is thrown and no action is taken

### Requirement: MassTransit transport uses RabbitMQ
All three contexts (Auth, Permission, Expense) SHALL use RabbitMQ as the MassTransit transport for cross-process messaging.

#### Scenario: Permission publishes to RabbitMQ
- **WHEN** the Permission Service publishes a role lifecycle event
- **THEN** the message is enqueued to a RabbitMQ exchange reachable by the Auth Service

#### Scenario: Auth consumes from RabbitMQ
- **WHEN** the Auth Service starts
- **THEN** it connects to RabbitMQ and registers consumers for role lifecycle events
- **THEN** incoming messages are processed by the corresponding consumers
