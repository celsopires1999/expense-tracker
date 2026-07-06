# Payment Methods

## Purpose
Define the payment methods that can be associated with expenses, enabling consistent payment tracking and reporting.

## Requirements

### Requirement: List payment methods
The system SHALL return all available payment methods. Any authenticated user SHALL be able to read payment methods.

#### Scenario: Default user lists payment methods
- **WHEN** a default user sends GET `/payment-methods`
- **THEN** the response returns 200 OK with the list of payment methods

### Requirement: Admin creates payment method
The system SHALL allow Admin to create a new payment method.

#### Scenario: Admin creates payment method
- **WHEN** an Admin sends POST `/payment-methods` with a valid name
- **THEN** the response returns 201 Created with the payment method ID

#### Scenario: Default user tries to create payment method
- **WHEN** a default user sends POST `/payment-methods`
- **THEN** the response returns 403 Forbidden

### Requirement: Admin updates payment method
The system SHALL allow Admin to update an existing payment method's name.

#### Scenario: Admin updates payment method
- **WHEN** an Admin sends PUT `/payment-methods/{id}` with a new name
- **THEN** the response returns 200 OK with the updated payment method

### Requirement: Admin deletes payment method
The system SHALL allow Admin to delete a payment method.

#### Scenario: Admin deletes unused payment method
- **WHEN** an Admin sends DELETE `/payment-methods/{id}` and no expenses reference it
- **THEN** the response returns 204 No Content

#### Scenario: Admin deletes payment method in use
- **WHEN** an Admin sends DELETE `/payment-methods/{id}` and expenses reference it
- **THEN** the response returns 409 Conflict

### Requirement: Payment method name uniqueness
The system SHALL enforce unique payment method names.

#### Scenario: Duplicate payment method name
- **WHEN** an Admin sends POST `/payment-methods` with a name that already exists
- **THEN** the response returns 409 Conflict

### Requirement: Seed default payment methods
The system SHALL seed default payment methods on first migration.

#### Scenario: Default payment methods exist after migration
- **WHEN** the database migration runs
- **THEN** payment methods like "Crédito", "Débito", "Dinheiro", "Pix", "Boleto" SHALL exist
