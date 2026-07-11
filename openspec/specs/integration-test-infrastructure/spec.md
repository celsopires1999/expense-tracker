## ADDED Requirements

### Requirement: Shared PostgreSQL test container
The test infrastructure SHALL provide a single PostgreSQL 17 container instance shared across all test assemblies via a static lazy singleton pattern.

#### Scenario: Container starts once
- **WHEN** any test assembly's first test class is instantiated
- **THEN** a PostgreSQL 17 container is started with database "integration_tests"
- **AND** the container instance is reused by all subsequent test classes across all assemblies

#### Scenario: Container connection string is accessible
- **WHEN** a test fixture needs the database connection string
- **THEN** the `PostgreSqlFixture.ConnectionString` property returns a valid Npgsql connection string pointing to the running container

### Requirement: RSA key pair generation for JWT testing
The test infrastructure SHALL generate an ephemeral RSA 2048-bit key pair per test run and provide PEM file paths for JWT signing and verification.

#### Scenario: Key pair is generated
- **WHEN** the test infrastructure initializes
- **THEN** an RSA 2048-bit key pair is generated
- **AND** the private key is written to a temporary PEM file accessible via `PostgreSqlFixture.PrivateKey`
- **AND** the public key is written to a temporary PEM file accessible via `PostgreSqlFixture.PublicKey`

#### Scenario: Key pair is cleaned up
- **WHEN** all tests in all assemblies complete
- **THEN** the temporary PEM files are deleted
- **AND** the RSA instance is disposed

### Requirement: EF Core migration execution in tests
The test infrastructure SHALL apply actual EF Core migrations (not `EnsureCreated`) to test databases, including seed data and MassTransit outbox tables.

#### Scenario: Migrations are applied for a DbContext
- **WHEN** `MigrationApplier.ApplyMigrationsAsync<TContext>` is called with a service provider
- **THEN** all pending EF Core migrations for `TContext` are applied
- **AND** seed data from migrations is present in the database
- **AND** MassTransit outbox tables (inbox_state, outbox_message, outbox_state) exist

### Requirement: Database cleanup between test classes
The test infrastructure SHALL truncate all domain tables with CASCADE and re-seed required data between test classes.

#### Scenario: Database is cleaned after a test class completes
- **WHEN** a test class finishes all its tests
- **THEN** all domain tables are truncated with CASCADE
- **AND** seed data (Auth roles, Permission roles + permissions) is re-inserted

### Requirement: Parallel execution safety
Test assemblies SHALL disable parallel test execution to prevent race conditions with shared database state.

#### Scenario: Tests within an assembly run sequentially
- **WHEN** a test assembly is executed
- **THEN** test classes within that assembly run one at a time (not in parallel)
- **AND** TRUNCATE + re-seed operations do not conflict between classes
