## Why

Usuários recém-registrados não recebem nenhuma role, ficando sem permissões e impossibilitados de usar o sistema. O sistema já possui as roles Admin (controle total) e Viewer (somente leitura), mas não há uma role padrão que permita ao usuário recém-registrado gerenciar seus próprios gastos imediatamente.

## What Changes

- Criar a role **Standard** (seed data + migration)
- Atribuir a role Standard automaticamente a todo novo usuário no registro
- Permissões da role Standard:
  - `expenses:create` — criar próprios gastos
  - `expenses:read` — ler próprios gastos
  - `expenses:update` — atualizar próprios gastos
  - `expenses:delete` — excluir próprios gastos
  - `categories:read` — ler categorias (necessário para categorizar gastos)
  - `payment-methods:read` — ler formas de pagamento (necessário para criar gastos)
  - `tags:read` — ler tags (necessário para associar a gastos)
  - `users:access` — acessar próprio perfil

## Capabilities

### New Capabilities

- `standard-role`: Role padrão atribuída no registro, com permissões para gerenciar próprios gastos e ler recursos de apoio (categorias, formas de pagamento, tags).

### Modified Capabilities

<!-- Nenhuma capability existente modificada -->
- *Nenhuma*

## Impact

- **Domain**: Nenhuma alteração — as entidades `Role`, `UserRole`, `RolePermission` já existem
- **Infrastructure**: Adicionar seed da nova role `Standard` e suas permissões em `RoleConfiguration`/`RolePermissionConfiguration`; nova migration
- **Application**: Modificar `RegisterUserCommandHandler` para atribuir a role Standard ao novo usuário
- **Tests**: Atualizar testes de arquitetura se necessário
