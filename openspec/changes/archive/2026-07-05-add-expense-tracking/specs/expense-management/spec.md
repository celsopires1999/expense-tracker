## ADDED Requirements

### Requirement: User creates an expense
The system SHALL allow an authenticated user to create an expense. The expense SHALL be owned by the authenticated user. Admin SHALL be able to create expenses for any user.

#### Scenario: Default user creates own expense
- **WHEN** a default user sends POST `/expenses` with valid body (description, amount, date, categoryId, paymentMethodId)
- **THEN** the expense is created and assigned to the authenticated user's ID
- **THEN** the response returns 201 Created with the expense ID

#### Scenario: Admin creates expense for another user
- **WHEN** an Admin sends POST `/expenses` with a `userId` field in the body
- **THEN** the expense is created and assigned to the specified userId
- **THEN** the response returns 201 Created

#### Scenario: Viewer tries to create expense
- **WHEN** a Viewer sends POST `/expenses`
- **THEN** the response returns 403 Forbidden

### Requirement: User reads own expenses
The system SHALL allow a user to list their own expenses. Admin and Viewer SHALL list all users' expenses.

#### Scenario: Default user lists own expenses
- **WHEN** a default user sends GET `/expenses`
- **THEN** the response returns 200 OK with only that user's expenses

#### Scenario: Admin lists all expenses
- **WHEN** an Admin sends GET `/expenses`
- **THEN** the response returns 200 OK with expenses from all users

#### Scenario: Viewer lists all expenses
- **WHEN** a Viewer sends GET `/expenses`
- **THEN** the response returns 200 OK with expenses from all users

### Requirement: User reads expense by ID
The system SHALL allow a user to read a single expense by ID. Users can only read their own expenses. Admin and Viewer can read any expense.

#### Scenario: Default user reads own expense
- **WHEN** a default user sends GET `/expenses/{id}` for an expense they own
- **THEN** the response returns 200 OK with the expense details

#### Scenario: Default user tries to read another user's expense
- **WHEN** a default user sends GET `/expenses/{id}` for an expense they do not own
- **THEN** the response returns 404 Not Found

#### Scenario: Admin reads any expense
- **WHEN** an Admin sends GET `/expenses/{id}` for any expense
- **THEN** the response returns 200 OK

### Requirement: User updates own expense
The system SHALL allow a user to update their own expenses. Admin SHALL update any expense. Fields: description, amount, date, categoryId, paymentMethodId.

#### Scenario: Default user updates own expense
- **WHEN** a default user sends PUT `/expenses/{id}` with valid fields for an expense they own
- **THEN** the response returns 200 OK with updated expense

#### Scenario: Default user tries to update another user's expense
- **WHEN** a default user sends PUT `/expenses/{id}` for an expense they do not own
- **THEN** the response returns 404 Not Found

#### Scenario: Admin updates any expense
- **WHEN** an Admin sends PUT `/expenses/{id}` for any expense
- **THEN** the response returns 200 OK

### Requirement: User deletes own expense
The system SHALL allow a user to delete their own expenses. Admin SHALL delete any expense.

#### Scenario: Default user deletes own expense
- **WHEN** a default user sends DELETE `/expenses/{id}` for an expense they own
- **THEN** the response returns 204 No Content

#### Scenario: Admin deletes any expense
- **WHEN** an Admin sends DELETE `/expenses/{id}` for any expense
- **THEN** the response returns 204 No Content

### Requirement: Expense validation
The system SHALL enforce business rules on expense fields.

#### Scenario: Description too short
- **WHEN** a user sends an expense with description less than 3 characters
- **THEN** the response returns 400 Bad Request with validation error

#### Scenario: Description too long
- **WHEN** a user sends an expense with description more than 255 characters
- **THEN** the response returns 400 Bad Request with validation error

#### Scenario: Amount is zero or negative
- **WHEN** a user sends an expense with amount <= 0
- **THEN** the response returns 400 Bad Request with validation error

#### Scenario: Category does not exist
- **WHEN** a user sends an expense with a non-existent categoryId
- **THEN** the response returns 400 Bad Request with validation error

#### Scenario: PaymentMethod does not exist
- **WHEN** a user sends an expense with a non-existent paymentMethodId
- **THEN** the response returns 400 Bad Request with validation error

### Requirement: Filter expenses by date range
The system SHALL allow filtering expenses by date range via query parameters.

#### Scenario: Filter by start and end date
- **WHEN** a user sends GET `/expenses?from=2024-01-01&to=2024-12-31`
- **THEN** the response returns 200 OK with expenses within that date range

#### Scenario: Filter by start date only
- **WHEN** a user sends GET `/expenses?from=2024-01-01`
- **THEN** the response returns 200 OK with expenses from that date onwards

### Requirement: Filter expenses by category
The system SHALL allow filtering expenses by category via query parameter.

#### Scenario: Filter by categoryId
- **WHEN** a user sends GET `/expenses?categoryId={guid}`
- **THEN** the response returns 200 OK with expenses matching that category

### Requirement: Filter expenses by tags
The system SHALL allow filtering expenses by tags via query parameter.

#### Scenario: Filter by tagId
- **WHEN** a user sends GET `/expenses?tagId={guid}`
- **THEN** the response returns 200 OK with expenses that have that tag

### Requirement: Add tags to expense
The system SHALL allow adding tags to an expense on creation and update.

#### Scenario: Create expense with tags
- **WHEN** a user sends POST `/expenses` with a `tagIds` array containing valid tag IDs
- **THEN** the expense is created with those tags attached

#### Scenario: Update expense tags
- **WHEN** a user sends PUT `/expenses/{id}` with a different `tagIds` array
- **THEN** the expense's tags are replaced with the new set
