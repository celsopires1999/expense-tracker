## 0. Preparação (dados descartáveis)

- [x] 0.1 Remover migration anterior: `dotnet ef migrations remove --project src/Infrastructure --startup-project src/Web.Api`
- [x] 0.2 Dropar banco: `dotnet ef database drop --project src/Infrastructure --startup-project src/Web.Api`
- [x] 0.3 Garantir banco recriado do zero (docker compose restart postgres ou drop manual)

## 1. Seed Data

- [x] 1.1 Add Standard role seed to `RoleConfiguration.cs` (ID fixo `33333333-3333-3333-3333-333333333333`, Name: `Standard`)
- [x] 1.2 Add Standard role permissions seed to `RolePermissionConfiguration.cs` (8 permissões)
- [x] 1.3 Generate EF migration: `dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api`
- [x] 1.4 Apply migration: `dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api`

## 2. Application Logic

- [x] 2.1 `IApplicationDbContext` já injetado — usar `context.Roles` e `context.UserRoles`
- [x] 2.2 Buscar role Standard por nome e criar `UserRole` no handler
- [x] 2.3 `SaveChangesAsync` é uma transação única — `User` e `UserRole` salvos juntos

## 3. Tests

- [x] 3.1 Verificar que registro atribui role Standard (teste de integração ou handler test)
- [x] 3.2 Verificar que role Standard tem as 8 permissões corretas (teste de seed)
- [x] 3.3 Executar `dotnet test` e confirmar que nada quebrou — **6/6 passed**

## 4. Verificação Final

- [x] 4.1 Buildar solução (`dotnet build ExpenseTracker.slnx`) sem erros — **0 errors**
- [x] 4.2 Rodar análise estática/linters — **build já inclui AnalysisMode=All, 0 erros**
- [x] 4.3 Atualizar `manual-tests/api.http` com comentários sobre permissões Standard
