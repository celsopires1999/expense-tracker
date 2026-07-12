## MODIFIED Requirements

### Requirement: Role GUIDs are deterministic
The Auth Service SHALL seed roles with deterministic GUIDs matching the Permission Service: Admin=11111111-1111-1111-1111-111111111111, Viewer=22222222-2222-2222-2222-222222222222, Standard=33333333-3333-3333-3333-333333333333. Additionally, the Auth Service SHALL consume role lifecycle events from the Permission Service to keep its Role table in sync with any roles created, updated, or deleted at runtime.

#### Scenario: Seed roles exist on startup
- **WHEN** the Auth Service database is migrated
- **THEN** three roles exist with the deterministic GUIDs and names "Admin", "Viewer", "Standard"

#### Scenario: Auth receives role created event
- **WHEN** the Auth Service receives a `RoleCreatedEvent` for a new role GUID
- **THEN** a new Role record is created in the Auth database with the event's GUID and name
- **THEN** the role is available for user-role assignments

#### Scenario: Auth receives role deleted event
- **WHEN** the Auth Service receives a `RoleDeletedEvent` for an existing role GUID
- **THEN** the Role record is deleted from the Auth database
- **THEN** any UserRole records referencing that role are cascade-deleted
