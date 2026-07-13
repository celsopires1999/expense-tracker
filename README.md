# ExpenseTracker

Personal expense tracking API built with .NET 10, following Clean Architecture (DDD + CQRS), split into three microservices.

## Technologies

- .NET 10
- PostgreSQL 17 (EF Core) — one database per service
- JWT authentication (RSA asymmetric signing)
- MassTransit (RabbitMQ transport + PostgreSQL outbox)
- Serilog + Seq
- Scrutor (convention-based DI registration + decoration)
- FluentValidation

## Architecture

```
src/
├── SharedKernel/         ← DDD abstractions (Entity, Result, IDomainEvent, IDateTimeProvider)
├── Auth/                 ← Auth Service — users, roles, JWT issuance
│   ├── Auth.Domain/
│   ├── Auth.Application/
│   ├── Auth.Infrastructure/
│   └── Auth.Api/
├── Permission/           ← Permission Service — roles, permissions, resolution
│   ├── Permission.Domain/
│   ├── Permission.Application/
│   ├── Permission.Infrastructure/
│   └── Permission.Api/
└── Expense/              ← Expense Tracker — business entities (expenses, categories, etc.)
    ├── Expense.Domain/
    ├── Expense.Application/
    ├── Expense.Infrastructure/
    └── Expense.Api/
```

### Cross-service communication

1. **Auth Service** issues RSA-signed JWTs (RS256) and exposes `GET /.well-known/jwks`
2. **Expense Service** validates JWTs by fetching keys from Auth's JWKS endpoint
3. **Expense Service** resolves permissions via `POST /permissions/resolve` on Permission Service
4. Results are cached in `IMemoryCache` (5 min TTL, keyed by `perms:{userId}`)
5. **Permission Service** publishes role change events (`RoleCreated/Updated/DeletedEvent`) via RabbitMQ
6. **Auth Service** consumes these events to keep its local `roles` table in sync

### Service matrix

| Service | Port | Database | Swagger |
|---------|------|----------|---------|
| Auth.Api | 5100 | `auth-db` | `/swagger` |
| Permission.Api | 5200 | `permission-db` | `/swagger` |
| Expense.Api | 5000 | `expense-db` | `/swagger` |
| PostgreSQL | 5432 | — | — |
| RabbitMQ | 5672 | — | `localhost:15672` (management) |
| Seq | 8082 | — | — |

## Prerequisites

- Docker (Compose V2)
- .NET 10 SDK
- `just` command runner (optional, see `justfile`)

## Quick start

### 1. Generate RSA keys

```bash
mkdir -p keys
openssl genrsa -out keys/auth-private.pem 2048
openssl rsa -in keys/auth-private.pem -pubout -out keys/auth-public.pem
```

### 2. Start infrastructure

```bash
docker compose up -d
```

### 3. Create databases

```bash
docker compose exec postgres psql -U postgres -c "CREATE DATABASE \"auth-db\";"
docker compose exec postgres psql -U postgres -c "CREATE DATABASE \"permission-db\";"
docker compose exec postgres psql -U postgres -c "CREATE DATABASE \"expense-db\";"
```

### 4. Apply migrations

```bash
dotnet ef database update --project src/Auth/Auth.Infrastructure --startup-project src/Auth/Auth.Api --context AuthDbContext
dotnet ef database update --project src/Permission/Permission.Infrastructure --startup-project src/Permission/Permission.Api --context PermissionDbContext
dotnet ef database update --project src/Expense/Expense.Infrastructure --startup-project src/Expense/Expense.Api --context ApplicationDbContext
```

### 5. Run all services

Open three terminals:

```bash
# Terminal 1 — Auth
dotnet run --project src/Auth/Auth.Api

# Terminal 2 — Permission
dotnet run --project src/Permission/Permission.Api

# Terminal 3 — Expense
dotnet run --project src/Expense/Expense.Api
```

## Endpoints by service

### Auth Service (`http://localhost:5100`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/auth/register` | No | Register new user (auto-assigns Standard role) |
| POST | `/auth/login` | No | Login, returns JWT |
| GET | `/auth/users` | Yes | List all users with roles |
| GET | `/auth/users/{id}` | Yes | Get user by ID |
| POST | `/auth/users/{id}/roles/{roleId}` | Yes | Assign role to user |
| DELETE | `/auth/users/{id}/roles/{roleId}` | Yes | Remove role from user |
| GET | `/.well-known/jwks` | No | JWKS public key set |

### Permission Service (`http://localhost:5200`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/permissions/roles` | Yes | Create role |
| GET | `/permissions/roles` | Yes | List all roles |
| GET | `/permissions/roles/{id}` | Yes | Get role with permissions |
| PUT | `/permissions/roles/{id}` | Yes | Update role name |
| PUT | `/permissions/roles/{id}/permissions` | Yes | Update role permissions |
| DELETE | `/permissions/roles/{id}` | Yes | Delete role |
| POST | `/permissions/resolve` | No | Resolve permissions from roles |

### Expense Service (`http://localhost:5000`)

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| GET | `/health` | No | Health check |
| POST | `/expenses` | `expenses:create` | Create expense |
| GET | `/expenses` | `expenses:read` | List expenses (with filters) |
| GET | `/expenses/{id}` | `expenses:read` | Get expense by ID |
| PUT | `/expenses/{id}` | `expenses:update` | Update expense |
| DELETE | `/expenses/{id}` | `expenses:delete` | Delete expense |
| POST | `/categories` | `categories:create` | Create category |
| GET | `/categories` | `categories:read` | List categories |
| PUT | `/categories/{id}` | `categories:update` | Update category |
| DELETE | `/categories/{id}` | `categories:delete` | Delete category |
| POST | `/payment-methods` | `payment-methods:create` | Create payment method |
| GET | `/payment-methods` | `payment-methods:read` | List payment methods |
| PUT | `/payment-methods/{id}` | `payment-methods:update` | Update payment method |
| DELETE | `/payment-methods/{id}` | `payment-methods:delete` | Delete payment method |
| POST | `/tags` | `tags:create` | Create tag |
| GET | `/tags` | `tags:read` | List tags |
| PUT | `/tags/{id}` | `tags:update` | Update tag |
| DELETE | `/tags/{id}` | `tags:delete` | Delete tag |

## Configuration

### Auth Service — `src/Auth/Auth.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "Database": "Host=host.docker.internal;Port=5432;Database=auth-db;..."
  },
  "Jwt": {
    "PrivateKeyPath": "/workspace/keys/auth-private.pem",
    "PublicKeyPath": "/workspace/keys/auth-public.pem",
    "Issuer": "auth-service",
    "Audience": "expense-tracker",
    "ExpirationInMinutes": 60
  },
  "RabbitMQ": {
    "Host": "host.docker.internal",
    "Port": 5672,
    "User": "guest",
    "Password": "guest"
  }
}
```

### Permission Service — `src/Permission/Permission.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "Database": "Host=host.docker.internal;Port=5432;Database=permission-db;..."
  },
  "RabbitMQ": {
    "Host": "host.docker.internal",
    "Port": 5672,
    "User": "guest",
    "Password": "guest"
  }
}
```

### Expense Service — `src/Expense/Expense.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "Database": "Host=host.docker.internal;Port=5432;Database=expense-db;..."
  },
  "Jwt": {
    "Issuer": "auth-service",
    "Audience": "expense-tracker",
    "JwksUrl": "http://localhost:5100/.well-known/jwks"
  },
  "PermissionService": {
    "BaseUrl": "http://localhost:5200"
  },
  "RabbitMQ": {
    "Host": "host.docker.internal",
    "Port": 5672,
    "User": "guest",
    "Password": "guest"
  }
}
```

## Running inside Docker (dev container)

```bash
docker compose up -d
docker compose exec -u developer -w /workspace app sh
# Inside the container:
just build
just api-expense   # runs Expense.Api on port 5000
```

## Useful commands

```bash
# Build
dotnet build ExpenseTracker.slnx

# Test
dotnet test ExpenseTracker.slnx

# Architecture tests only
dotnet test tests/ArchitectureTests

# Format
dotnet format ExpenseTracker.slnx

# Clean
dotnet clean ExpenseTracker.slnx

# List all justfile recipes
just --list
```

## Seed roles

Deterministic GUIDs used across Auth and Permission:

| Role | GUID |
|------|------|
| Admin | `11111111-1111-1111-1111-111111111111` |
| Viewer | `22222222-2222-2222-2222-222222222222` |
| Standard | `33333333-3333-3333-3333-333333333333` |

Standard role is auto-assigned to new users on registration.
