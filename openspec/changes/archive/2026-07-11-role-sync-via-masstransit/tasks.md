## 1. Infrastructure — RabbitMQ Setup

- [x] 1.1 Add RabbitMQ service to `docker-compose.yml` (image: rabbitmq:3-management, ports: 5672, 15672)
- [x] 1.2 Add health check and dependency to `docker-compose.override.yml`
- [x] 1.3 Add `MassTransit.RabbitMQ` NuGet package to `Auth.Api`, `Permission.Api`, `Expense.Api` projects
- [x] 1.4 Add RabbitMQ configuration section to each service's `appsettings.json` (`RabbitMQ:Host`, `RabbitMQ:Port`, `RabbitMQ:User`, `RabbitMQ:Password`)

## 2. SharedKernel — Event Contracts

- [x] 2.1 Create `RoleCreatedEvent` record in `SharedKernel/Messaging/` with properties: `Guid Id`, `string Name`, `DateTime CreatedOn`
- [x] 2.2 Create `RoleUpdatedEvent` record in `SharedKernel/Messaging/` with properties: `Guid Id`, `string Name`, `DateTime UpdatedOn`
- [x] 2.3 Create `RoleDeletedEvent` record in `SharedKernel/Messaging/` with properties: `Guid Id`, `DateTime DeletedOn`

## 3. Permission Context — Domain Events

- [x] 3.1 Create `RoleCreatedDomainEvent` record in `Permission.Domain/Roles/` with property: `Guid RoleId`
- [x] 3.2 Create `RoleUpdatedDomainEvent` record in `Permission.Domain/Roles/` with property: `Guid RoleId`
- [x] 3.3 Create `RoleDeletedDomainEvent` record in `Permission.Domain/Roles/` with property: `Guid RoleId`

## 4. Permission Context — Events Dispatcher

- [x] 4.1 Create `IDomainEventsDispatcher` interface in `Permission.Infrastructure/DomainEvents/` (copy from Expense)
- [x] 4.2 Create `DomainEventsDispatcher` implementation in `Permission.Infrastructure/DomainEvents/` (copy from Expense, adapt for Permission's DI scope)
- [x] 4.3 Override `SaveChangesAsync` in `PermissionDbContext` to extract and dispatch domain events after save (copy pattern from Expense's `ApplicationDbContext`)
- [x] 4.4 Register `IDomainEventsDispatcher` in `Permission.Infrastructure.DependencyInjection`

## 5. Permission Context — Command Handlers

- [x] 5.1 Update `CreateRoleCommandHandler` to call `role.Raise(new RoleCreatedDomainEvent(role.Id))` before `SaveChangesAsync`
- [x] 5.2 Update `DeleteRoleCommandHandler` to call `role.Raise(new RoleDeletedDomainEvent(role.Id))` before `SaveChangesAsync`
- [x] 5.3 Create or update `UpdateRoleCommand` and handler to call `role.Raise(new RoleUpdatedDomainEvent(role.Id))` before `SaveChangesAsync`
- [x] 5.4 Create `RoleCreatedEventHandler` (domain event handler) that maps `RoleCreatedDomainEvent` → `RoleCreatedEvent` and publishes via `IPublishEndpoint`
- [x] 5.5 Create `RoleUpdatedEventHandler` that maps `RoleUpdatedDomainEvent` → `RoleUpdatedEvent` and publishes via `IPublishEndpoint`
- [x] 5.6 Create `RoleDeletedEventHandler` that maps `RoleDeletedDomainEvent` → `RoleDeletedEvent` and publishes via `IPublishEndpoint`

## 6. Permission Context — MassTransit Transport

- [x] 6.1 Update `Permission.Infrastructure.DependencyInjection` MassTransit config: replace `UsingInMemory` with `UsingRabbitMQ` and configure connection from `appsettings.json`
- [x] 6.2 Register domain event handlers as MassTransit consumers (or use `IPublishEndpoint` directly —取决于设计)

## 7. Auth Context — MassTransit Consumer

- [x] 7.1 Create `RoleCreatedConsumer` in `Auth.Infrastructure/Roles/` implementing `IConsumer<RoleCreatedEvent>` — upserts Role by GUID
- [x] 7.2 Create `RoleUpdatedConsumer` in `Auth.Infrastructure/Roles/` implementing `IConsumer<RoleUpdatedEvent>` — updates Role name by GUID
- [x] 7.3 Create `RoleDeletedConsumer` in `Auth.Infrastructure/Roles/` implementing `IConsumer<RoleDeletedEvent>` — deletes Role by GUID (cascade to UserRoles)
- [x] 7.4 Register all three consumers in `Auth.Infrastructure.DependencyInjection` MassTransit config
- [x] 7.5 Update `Auth.Infrastructure.DependencyInjection` MassTransit config: replace `UsingInMemory` with `UsingRabbitMQ` and configure connection

## 8. Expense Context — MassTransit Transport

- [x] 8.1 Update `Expense.Infrastructure.DependencyInjection` MassTransit config: replace `UsingInMemory` with `UsingRabbitMQ` and configure connection (no consumers needed now)

## 9. EF Migrations

- [x] 9.1 Verify Permission context outbox migration is up to date (no schema change expected — outbox tables already exist)
- [x] 9.2 Verify Auth context outbox migration is up to date
- [x] 9.3 Verify Expense context outbox migration is up to date

## 10. Testing & Verification

- [x] 10.1 Write integration test: create role in Permission.Api → verify Role exists in Auth database
- [x] 10.2 Write integration test: delete role in Permission.Api → verify Role deleted from Auth database
- [x] 10.3 Write integration test: update role in Permission.Api → verify Role name updated in Auth database
- [x] 10.4 Verify `dotnet build ExpenseTracker.slnx` passes
- [x] 10.5 Verify `dotnet test ExpenseTracker.slnx` passes (ArchitectureTests: 6/6 passed)
- [x] 10.6 Manual test: start all services + RabbitMQ, create a role via Permission.Api, verify it appears in Auth.Api's role list
