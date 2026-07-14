## 1. Domain Layer

- [x] 1.1 Create `ExpenseStatus` enum in Expense.Domain with values: Pending, Approved, Rejected, Paid
- [x] 1.2 Add `Status` property to `Expense` entity
- [x] 1.3 Add status transition validation method to `Expense` entity

## 2. Application Layer

- [x] 2.1 Update `CreateExpenseCommand` to set Status = Pending by default
- [x] 2.2 Update `UpdateExpenseCommand` to accept optional Status field
- [x] 2.3 Add Admin-only status change validation in `UpdateExpenseCommandHandler`
- [x] 2.4 Update `GetExpensesQuery` to support status filter parameter
- [x] 2.5 Update DTOs to include Status field

## 3. Infrastructure Layer

- [x] 3.1 Create EF migration for ExpenseStatus column
- [x] 3.2 Update `ApplicationDbContext` with Status configuration
- [x] 3.3 Set existing expenses to "Paid" status in migration

## 4. API Layer

- [x] 4.1 Update Expense endpoints to return status in responses
- [x] 4.2 Add status filter parameter to GET `/expenses` endpoint
- [x] 4.3 Add status validation in PUT `/expenses/{id}` endpoint

## 5. Testing

- [x] 5.1 Add unit tests for status transition validation
- [x] 5.2 Add integration tests for status CRUD operations
- [x] 5.3 Add integration tests for status filtering
