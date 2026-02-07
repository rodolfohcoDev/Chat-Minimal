# Plano de Implementação - Migração SQL Server e Configuração Dinâmica de IA

## Objetivo
Migrar o projeto `Chat.Minimal.Services` de MySQL para SQL Server e implementar um sistema de configuração dinâmica de IA com controle de uso, limites de tokens e logging completo de interações.

## Status Geral: 🟢 EM PROGRESSO

---

## Fase 1: Migração para SQL Server ✅ CONCLUÍDA

### 1.1 Atualizar Dependências ✅
- [x] Remover `Pomelo.EntityFrameworkCore.MySql`
- [x] Adicionar `Microsoft.EntityFrameworkCore.SqlServer`

### 1.2 Atualizar Configurações ✅
- [x] Atualizar `appsettings.json` com connection string SQL Server
- [x] Atualizar `appsettings.Development.json` com connection string SQL Server
- [x] Atualizar `Program.cs` para usar `UseSqlServer`
- [x] Atualizar `ApplicationDbContextFactory.cs` para SQL Server

### 1.3 Migrations ✅
- [x] Remover migrations antigas do MySQL
- [x] Criar migration inicial para SQL Server
- [x] Aplicar migration no banco de dados

---

## Fase 2: Entidades de Configuração e Logging ✅ CONCLUÍDA

### 2.1 Criar Entidades ✅
- [x] `AiConfig` - Configuração de IA (provider, model, API key, limites, expiração)
- [x] `AiInteractionLog` - Log de interações (request, response, tokens, duração)

### 2.2 Atualizar DbContext ✅
- [x] Adicionar `DbSet<AiConfig>` e `DbSet<AiInteractionLog>`
- [x] Configurar entidades no `OnModelCreating`
- [x] Definir tipos de dados apropriados (NVARCHAR(MAX) para textos longos)

---

## Fase 3: Serviço de Orquestração ✅ CONCLUÍDA

### 3.1 Criar DTOs ✅
- [x] `LlmResponse` - DTO com conteúdo e metadados (tokens, provider, model)

### 3.2 Criar Interface e Implementação ✅
- [x] `IAiOrchestrator` - Interface do orquestrador
- [x] `AiOrchestrator` - Implementação com:
  - Busca de configuração ativa do banco
  - Validação de expiração
  - Validação de limites de tokens
  - Logging de request/response completo
  - Atualização de contadores de uso
  - Tratamento de erros com logging

### 3.3 Registrar Serviços ✅
- [x] Registrar `AiOrchestrator` no DI container

---

## Fase 4: Seed de Dados ✅ CONCLUÍDA

### 4.1 Scripts de Seed ✅
- [x] Criar `SeedGroqConfig.sql` - Script SQL para inserir configuração
- [x] Criar `SeedGroqConfig.cs` - Método C# para seed
- [x] Atualizar `Program.cs` para executar seed no startup (Development)

---

## Fase 5: Integração com Sistema Existente 🟡 EM ANDAMENTO

### 5.1 Atualizar Handlers CQRS
- [ ] Criar novo `AskQuestionCommandHandler` que usa `AiOrchestrator`
- [ ] Ou adaptar handler existente para usar o orquestrador
- [ ] Atualizar `AnswerDto` para incluir metadados de tokens

### 5.2 Atualizar Serviços de IA
- [ ] Modificar `GroqService` para retornar metadados de tokens
- [ ] Atualizar `ILlmService` se necessário para suportar metadados
- [ ] Ou criar wrapper que extrai metadados da resposta da API

### 5.3 Endpoints
- [ ] Verificar se endpoints precisam de ajustes
- [ ] Adicionar endpoint para consultar uso de tokens
- [ ] Adicionar endpoint para gerenciar configurações de IA (CRUD)

---

## Fase 6: Testes e Validação 🔴 PENDENTE

### 6.1 Testes Funcionais
- [ ] Testar criação de conversa
- [ ] Testar envio de mensagem e resposta da IA
- [ ] Verificar se logs estão sendo criados corretamente
- [ ] Verificar se contadores de tokens estão sendo atualizados

### 6.2 Testes de Limites
- [ ] Testar comportamento quando limite de tokens é atingido
- [ ] Testar comportamento quando configuração está expirada
- [ ] Testar comportamento quando não há configuração ativa

### 6.3 Testes de Performance
- [ ] Verificar impacto no tempo de resposta
- [ ] Verificar se logging está performático

---

## Fase 7: Melhorias Futuras 🔵 PLANEJADO

### 7.1 Tokenização Precisa
- [ ] Integrar biblioteca de tokenização (tiktoken ou similar)
- [ ] Substituir estimativa simples por contagem real de tokens

### 7.2 Múltiplas Configurações
- [ ] Implementar seleção de configuração por usuário
- [ ] Implementar fallback entre configurações
- [ ] Implementar balanceamento de carga entre configurações

### 7.3 Dashboard de Monitoramento
- [ ] Criar endpoint para estatísticas de uso
- [ ] Criar endpoint para histórico de interações
- [ ] Adicionar métricas de custo estimado

### 7.4 Gerenciamento de Configurações
- [ ] Criar endpoints CRUD para `AiConfig`
- [ ] Adicionar validação de API keys
- [ ] Implementar rotação automática de keys

---

## Próximos Passos Imediatos

1. **Testar a aplicação**
   - Executar a aplicação
   - Verificar se seed foi executado
   - Testar endpoint de chat

2. **Integrar com CQRS**
   - Decidir estratégia de integração (modificar handler existente ou criar novo)
   - Implementar integração escolhida

3. **Atualizar serviços de IA**
   - Modificar `GroqService` para retornar metadados
   - Ou criar adapter/wrapper

4. **Testes completos**
   - Validar todos os cenários
   - Verificar logs e contadores

---

## Notas Técnicas

### Estimativa de Tokens
Atualmente usando estimativa simples (4 caracteres = 1 token). Para produção, considerar:
- **tiktoken** (OpenAI) - Mais preciso para modelos GPT
- **sentencepiece** - Para modelos Llama
- API própria do provider (se disponível)

### Segurança
- API keys armazenadas em texto plano no banco
- **TODO**: Implementar criptografia para API keys sensíveis

### Performance
- Queries otimizadas com índices em `AiInteractionLogs`
- Considerar particionamento de tabela de logs se volume crescer muito

### Observabilidade
- Logs estruturados com `ILogger`
- Métricas de duração capturadas
- Considerar integração com Application Insights ou similar
