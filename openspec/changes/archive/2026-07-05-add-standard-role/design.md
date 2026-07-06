## Context

O sistema já possui RBAC completo com entidades `Role`, `UserRole`, `RolePermission`. Atualmente existem duas roles seedadas: Admin e Viewer. Usuários registrados não recebem nenhuma role automaticamente, ficando sem permissões.

A mudança adiciona uma terceira role — Standard — atribuída automaticamente no registro, com permissões para gerenciar próprios gastos e ler recursos de apoio.

## Goals / Non-Goals

**Goals:**
- Criar a role Standard no seed de dados (ef core migration)
- Atribuir a role Standard automaticamente no `RegisterUserCommandHandler`
- Garantir que o `PermissionAuthorizationHandler` funcione sem alterações (já consulta UserRole dinamicamente)
- Incluir `users:access` para permitir acesso ao próprio perfil

**Non-Goals:**
- Não alterar a estrutura de domínio (entidades já existem)
- Não criar endpoints de gerenciamento de roles (admin feature futura)
- Não alterar as roles Admin ou Viewer

## Decisions

1. **Role atribuída no comando de registro, não no seed**
   - Alternativa: criar trigger no banco ou no construtor da entidade
   - Decisão: o `RegisterUserCommandHandler` é o ponto natural — insere o `User` e já adiciona o `UserRole`. Simples, explícito, testável.

2. **Sem nova migration para o seed**
   - A role Standard será adicionada via `HasData` no `RoleConfiguration` e `RolePermissionConfiguration`
   - Isso gera uma nova migration, que segue o fluxo normal de EF Core

3. **`PermissionVersion` não precisa ser alterado no registro**
   - A role é atribuída antes do primeiro acesso do usuário, então o cache de permissões já estará populado corretamente na primeira consulta
   - O `perm_version` no JWT já está em 0 (default), que é o valor inicial do banco

## Risks / Trade-offs

- [Role fixa no registro] → Se no futuro houver múltiplas roles padrão (ex: por plano de conta), precisará de lógica de seleção; por enquanto uma única role resolve
- [Nova migration com seed] → Sem risco — projeto em fase inicial, sem concorrência de migrations
- [Usuários existentes sem role] → Esta change não trata migração de usuários existentes; podem continuar sem role até que um admin (ou futuro script) atribua manualmente
