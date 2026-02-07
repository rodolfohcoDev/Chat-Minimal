# Processo de Onboard de Aplicação

Este documento descreve o processo de integração (onboard) de uma nova aplicação cliente ou usuário administrativo no sistema Chat Minimal.

## Visão Geral

O sistema utiliza controle de acesso baseado em Roles (RBAC) e autenticação via API Keys. Integrado ao ASP.NET Core Identity, o sistema mapeia a API Key fornecida para um Usuário e suas respectivas Roles.

### Tipos de Usuários e Permissões

1.  **Administrator**
    -   Role: `Administrator`
    -   Permissões: Acesso total ao sistema, configurações de IA (`AiConfigs`), gerenciamento de usuários e visualização de logs.
    
2.  **Application User (Service Account)**
    -   Role: `application_user`
    -   Permissões: Acesso restrito aos endpoints de negócio (ex: Chat, Tasks). Ideal para integrações M2M (Machine-to-Machine).

### Fluxo de Autenticação e Autorização

1.  O cliente envia a API Key no header HTTP `X-API-Key`.
2.  O Middleware valida a existência e validade da chave.
3.  O sistema identifica o Usuário dono da chave.
4.  O sistema carrega as Roles desse usuário (`Administrator` ou `application_user`) através do `UserManager`.
5.  As Roles são adicionadas às Claims do contexto HTTP.
6.  Endpoints protegidos com `[Authorize(Roles = "Administrator")]` ou `application_user` validam o acesso.

## Como Realizar o Onboard

### Passo 1: Executar Script de Setup

Foi criado um script SQL automatizado para realizar o setup inicial das roles e usuários padrão.

- **Localização**: `src/src/app/Chat.Minimal.Services.api/Scripts/OnboardApplicationUser.sql`

**O que o script faz:**
1.  Garante que as tabelas `AspNetRoles` contenham `Administrator` e `application_user`.
2.  Cria o usuário Admin padrão (`admin@sistema.com`) se não existir.
3.  Cria o usuário de Aplicação padrão (`app-integration@sistema.com`) se não existir.
4.  Associa cada usuário à sua Role respectiva na tabela `AspNetUserRoles`.
5.  Gera uma nova API Key válida por 1 ano para o usuário de aplicação e a registra na tabela `ApiKeys`.

### Passo 2: Execução no Banco de Dados

Conecte-se ao seu banco de dados SQL Server e execute o script.

**Exemplo de Saída no Console/Mensagens:**
```text
---------------------------------------------------
ONBOARDING CONCLUÍDO COM SUCESSO
---------------------------------------------------
Admin User: admin@sistema.com
App User:   app-integration@sistema.com
App Role:   application_user
API Key:    sk-app-a1b2c3d4...
---------------------------------------------------
```

A API Key gerada será exibida nos resultados da query. **Copie e armazene esta chave com segurança.**

## Validação

Para validar se o usuário de aplicação tem as permissões corretas, faça uma requisição para um endpoint protegido usando a chave gerada.

**Exemplo de Requisição:**

```bash
curl --location 'http://localhost:5120/api/chat/task' \
--header 'Content-Type: application/json' \
--header 'X-API-Key: SUA_CHAVE_GERADA_AQUI' \
--data '{
    "question": "Teste de permissão",
    "conversationId": "test-001"
}'
```

## Manutenção de Acessos

- **Revogação**: Para remover o acesso de uma aplicação, execute um `UPDATE ApiKeys SET IsActive = 0 WHERE Key = '...'`. O usuário perderá acesso imediato, mesmo que seu usuário continue ativo.
- **Rotação de Chaves**: Para rotacionar, gere uma nova chave usando o script (ou endpoint de geração) para o mesmo `UserId` e invalide a anterior.
- **Alterar Permissões**: Para promover um usuário a Admin, insira um registro na tabela `AspNetUserRoles` associando o `UserId` ao `RoleId` de Administrador.
