## ADDED Requirements

### Requirement: Expense has status
Each expense SHALL have a status field with one of the following values: Pending, Approved, Rejected, or Paid.

#### Scenario: New expense defaults to Pending
- **WHEN** a user creates an expense via POST `/expenses`
- **THEN** the expense status is set to "Pending"

#### Scenario: Status is included in response
- **WHEN** a user retrieves an expense via GET `/expenses/{id}`
- **THEN** the response includes a `status` field with one of: "Pending", "Approved", "Rejected", "Paid"

### Requirement: Admin changes expense status
Admin SHALL be able to change the status of any expense. Default users SHALL NOT be able to change status.

#### Scenario: Admin changes status to Approved
- **WHEN** an Admin sends PUT `/expenses/{id}` with `status: "Approved"` for an expense in "Pending" status
- **THEN** the expense status changes to "Approved"

#### Scenario: Admin changes status to Rejected
- **WHEN** an Admin sends PUT `/expenses/{id}` with `status: "Rejected"` for an expense in "Pending" status
- **THEN** the expense status changes to "Rejected"

#### Scenario: Admin changes status to Paid
- **WHEN** an Admin sends PUT `/expenses/{id}` with `status: "Paid"` for an expense in "Approved" status
- **THEN** the expense status changes to "Paid"

#### Scenario: Default user tries to change status
- **WHEN** a default user sends PUT `/expenses/{id}` with a `status` field
- **THEN** the response returns 403 Forbidden

#### Scenario: Invalid status transition
- **WHEN** an Admin sends PUT `/expenses/{id}` with `status: "Paid"` for an expense in "Pending" status
- **THEN** the response returns 400 Bad Request with validation error

### Requirement: Filter expenses by status
The system SHALL allow filtering expenses by status via query parameter.

#### Scenario: Filter by status
- **WHEN** a user sends GET `/expenses?status=Pending`
- **THEN** the response returns 200 OK with only expenses in "Pending" status

#### Scenario: Filter by multiple statuses
- **WHEN** a user sends GET `/expenses?status=Pending&status=Approved`
- **THEN** the response returns 200 OK with expenses in "Pending" or "Approved" status
