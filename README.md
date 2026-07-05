# ExpenseTracker

Personal expense tracking API built with .NET 10, following Clean Architecture (DDD + CQRS).

## Technologies

- .NET 10
- PostgreSQL (EF Core)
- JWT authentication
- Serilog + Seq

## Architecture

The solution follows Domain-Driven Design with CQRS, organized in 5 projects:

- **SharedKernel** — DDD abstractions (Entity, Error, Result, domain events)
- **Domain** — entities, value objects, domain events
- **Application** — CQRS commands/queries, handlers, validation
- **Infrastructure** — EF Core, authentication, authorization, logging
- **Web.Api** — Minimal API endpoints

## Running locally

```bash
docker compose up -d
dotnet run --project src/Web.Api/Web.Api.csproj
```
