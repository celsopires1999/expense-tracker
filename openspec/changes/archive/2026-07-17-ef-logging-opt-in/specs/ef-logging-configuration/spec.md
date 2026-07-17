## ADDED Requirements

### Requirement: EF Core logs suppressed by default in development
The system SHALL suppress Entity Framework Core log output at `Warning` level in all development API configurations, preventing SQL command logging, connection lifecycle events, and query compilation details from appearing in console or Seq sinks.

#### Scenario: EF logs not visible at default configuration
- **WHEN** the application starts with the default `appsettings.Development.json` configuration
- **THEN** no log events from `Microsoft.EntityFrameworkCore` or `Microsoft.EntityFrameworkCore.Database.Command` categories appear at `Information` or `Debug` level in console output or Seq

#### Scenario: Non-EF Microsoft logs preserved
- **WHEN** the application starts with the default `appsettings.Development.json` configuration
- **THEN** log events from `Microsoft.AspNetCore` and other non-EF Microsoft namespaces continue to appear at `Information` level as before

### Requirement: Developer can opt in to verbose EF logging
The system SHALL provide a documented configuration mechanism for developers to enable detailed EF Core logging when needed for debugging.

#### Scenario: Opt in via configuration override
- **WHEN** a developer sets `Microsoft.EntityFrameworkCore` to `Information` or `Debug` in the `Override` section of `appsettings.Development.json`
- **THEN** verbose EF Core logs (SQL queries, connection events, query compilation) appear in console and Seq sinks

#### Scenario: Opt in is documented inline
- **WHEN** a developer inspects any API's `appsettings.Development.json`
- **THEN** commented-out configuration lines are present showing how to re-enable EF Core logging
