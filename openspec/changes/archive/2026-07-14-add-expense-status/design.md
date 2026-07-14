## Context

Expense entity currently has: Description, Amount, Date, CategoryId, PaymentMethodId, Tags. No lifecycle state tracking exists. Users need to track expense approval workflow.

## Goals / Non-Goals

**Goals:**
- Add status field with enum: Pending, Approved, Rejected, Paid
- Default new expenses to Pending
- Allow Admin to transition status
- Enable filtering by status

**Non-Goals:**
- Approval workflow automation (manual status changes only)
- Status history/audit trail
- Notifications on status change

## Decisions

**Decision 1: ExpenseStatus as domain enum**
- Use `ExpenseStatus` enum in Expense.Domain with values: Pending=0, Approved=1, Rejected=2, Paid=3
- Rationale: Strongly typed, maps to PostgreSQL enum or int column via EF

**Decision 2: Status transitions enforced in domain**
- Domain entity validates allowed transitions: Pending→Approved/Rejected, Approved→Paid
- Invalid transitions throw DomainException
- Rationale: Business rules belong in domain layer

**Decision 3: Default status on creation**
- CreateExpenseCommand handler sets Status = Pending automatically
- User cannot specify status on creation
- Rationale: Simplifies API, enforces workflow start point

**Decision 4: Admin-only status changes**
- UpdateExpenseCommand includes optional Status field
- Only Admin role can modify status
- Default users get 403 if they try to change status

## Risks / Trade-offs

**Risk:** Existing expenses have no status → **Mitigation:** Migration sets all existing expenses to "Paid" (assuming they're completed)

**Risk:** Status column NOT NULL constraint → **Mitigation:** Default value in migration, then set existing rows

## Migration Plan

1. Add ExpenseStatus enum to PostgreSQL
2. Add Status column with default 'Pending'
3. UPDATE existing rows to 'Paid'
4. Add NOT NULL constraint
