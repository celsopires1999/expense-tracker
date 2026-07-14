## Why

Expenses currently lack a lifecycle state. Users need to track whether an expense is pending approval, approved, rejected, or paid. This enables approval workflows and better expense tracking.

## What Changes

- Add `status` field to Expense entity with enum values: Pending, Approved, Rejected, Paid
- New expense defaults to `Pending` status
- Status transitions: Pending → Approved/Rejected, Approved → Paid
- Admin can change status; default users cannot
- Filter expenses by status

## Capabilities

### New Capabilities

None - status is part of existing expense management.

### Modified Capabilities

- `expense-management`: Status field added to expense entity, creation defaults to Pending, update allows status changes by Admin, new filter by status

## Impact

- Expense.Domain: New `ExpenseStatus` enum, `Status` property on `Expense` entity
- Expense.Application: Update commands/queries for status handling
- Expense.Infrastructure: EF migration for new column
- Expense.Api: New filter parameter, status in responses
- Breaking: API responses include new `status` field (additive, not breaking)
