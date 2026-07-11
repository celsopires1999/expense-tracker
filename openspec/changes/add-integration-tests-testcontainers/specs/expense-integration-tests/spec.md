## ADDED Requirements

### Requirement: Expense handler-level integration tests
The Expense integration test project SHALL verify all CQRS handlers against a real PostgreSQL database with a real Permission service (via WebApplicationFactory).

#### Scenario: CreateCategoryCommand creates category
- **WHEN** `CreateCategoryCommandHandler` handles a valid command
- **THEN** a category record exists in the database with the provided name
- **AND** the returned GUID matches the created category's Id

#### Scenario: CreateCategoryCommand rejects duplicate name
- **WHEN** `CreateCategoryCommandHandler` handles a command with an existing category name
- **THEN** the result is a Conflict error (`Categories.DuplicateName`)

#### Scenario: DeleteCategoryCommand rejects category in use
- **WHEN** `DeleteCategoryCommandHandler` handles a command for a category referenced by expenses
- **THEN** the result is a Conflict error (`Categories.InUse`)

#### Scenario: CreateExpenseCommand creates expense with domain event
- **WHEN** `CreateExpenseCommandHandler` handles a valid command
- **THEN** an expense record exists in the database
- **AND** an `ExpenseCreatedDomainEvent` is raised on the entity
- **AND** the outbox_message table contains a record for the domain event

#### Scenario: CreateExpenseCommand validates referenced entities exist
- **WHEN** `CreateExpenseCommandHandler` handles a command with non-existent CategoryId or PaymentMethodId
- **THEN** the result is a NotFound error

#### Scenario: CreateExpenseCommand enforces ownership
- **WHEN** `CreateExpenseCommandHandler` handles a command where `IUserContext.UserId` differs from `command.UserId`
- **THEN** the result is a Failure error (`Expenses.Unauthorized`)

#### Scenario: UpdateExpenseCommand raises domain event
- **WHEN** `UpdateExpenseCommandHandler` handles a valid command
- **THEN** an `ExpenseUpdatedDomainEvent` is raised
- **AND** the outbox_message table contains a record for the domain event

#### Scenario: DeleteExpenseCommand raises domain event
- **WHEN** `DeleteExpenseCommandHandler` handles a valid command for an existing expense
- **THEN** an `ExpenseDeletedDomainEvent` is raised
- **AND** the outbox_message table contains a record for the domain event

#### Scenario: CreatePaymentMethodCommand creates payment method
- **WHEN** `CreatePaymentMethodCommandHandler` handles a valid command
- **THEN** a payment method record exists in the database

#### Scenario: CreateTagCommand creates tag
- **WHEN** `CreateTagCommandHandler` handles a valid command
- **THEN** a tag record exists in the database

### Requirement: Expense endpoint-level integration tests
The Expense integration test project SHALL verify all HTTP endpoints via `WebApplicationFactory<Program>` with real PostgreSQL and a real Permission service.

#### Scenario: All endpoints require authentication
- **WHEN** any Expense endpoint is called without a JWT token
- **THEN** the response is 401 Unauthorized

#### Scenario: POST /categories returns 201 with valid permissions
- **WHEN** a request with a valid JWT (containing `categories:create` permission) is sent to `POST /categories`
- **THEN** the response is 201 Created with the new category GUID

#### Scenario: POST /expenses returns 201 with valid permissions
- **WHEN** a request with a valid JWT (containing `expenses:create` permission) is sent to `POST /expenses`
- **THEN** the response is 201 Created with the new expense GUID

#### Scenario: Expense authorization uses real Permission service
- **WHEN** an Expense endpoint is called with a JWT containing role claims
- **THEN** the PermissionServiceClient calls the real Permission API's `/permissions/resolve` endpoint
- **AND** the authorization handler checks the resolved permissions against the required permission

### Requirement: Domain event outbox verification
The Expense integration test project SHALL verify that domain events are captured by the MassTransit outbox after `SaveChangesAsync`.

#### Scenario: Outbox captures ExpenseCreatedDomainEvent
- **WHEN** a `CreateExpenseCommand` is handled and saved
- **THEN** the `outbox_message` table contains a record with `message_type` containing "ExpenseCreatedDomainEvent"
- **AND** the record's payload contains the expense's Id

#### Scenario: Outbox captures ExpenseUpdatedDomainEvent
- **WHEN** an `UpdateExpenseCommand` is handled and saved
- **THEN** the `outbox_message` table contains a record with `message_type` containing "ExpenseUpdatedDomainEvent"

#### Scenario: Outbox captures ExpenseDeletedDomainEvent
- **WHEN** a `DeleteExpenseCommand` is handled and saved
- **THEN** the `outbox_message` table contains a record with `message_type` containing "ExpenseDeletedDomainEvent"
