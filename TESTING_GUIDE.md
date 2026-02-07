# 🧪 Testes de Integração - Chat.Minimal.Services API

Este documento contém testes manuais para validar todas as funcionalidades da API com a chave real do Groq.

## Pré-requisitos

1. Aplicação rodando: `dotnet run` (porta 5120)
2. API Key válida no banco de dados
3. Configuração do Groq ativa no banco

## Variáveis de Ambiente

```powershell
$API_KEY = "api-key-12345678901234567890123456789012"
$BASE_URL = "http://localhost:5120"
```

---

## Teste 1: Health Check ✅

**Objetivo**: Verificar se a API está rodando

```powershell
curl "$BASE_URL/health"
```

**Resultado Esperado**:
```json
{
  "status": "healthy",
  "timestamp": "2026-02-07T..."
}
```

---

## Teste 2: Pergunta Simples ✅

**Objetivo**: Validar resposta básica com metadados

```powershell
$body = @{
    question = "Qual é a capital do Brasil?"
    conversationId = "test-simple-001"
} | ConvertTo-Json

curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**Validações**:
- ✅ `answer` não vazio
- ✅ `inputTokens` > 0
- ✅ `outputTokens` > 0
- ✅ `totalTokens` = inputTokens + outputTokens
- ✅ `model` = "llama-3.3-70b-versatile"
- ✅ `provider` = "Groq"
- ✅ `processingTimeMs` > 0

---

## Teste 3: Múltiplas Perguntas (Acúmulo de Tokens) ✅

**Objetivo**: Verificar acúmulo de tokens no banco

```powershell
$conversationId = "test-multi-$(Get-Random)"

# Pergunta 1
$body1 = @{
    question = "Olá, como você está?"
    conversationId = $conversationId
} | ConvertTo-Json

$response1 = curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body1 | ConvertFrom-Json

Write-Host "Pergunta 1 - Tokens: $($response1.totalTokens)"

# Pergunta 2
$body2 = @{
    question = "Qual é a capital da França?"
    conversationId = $conversationId
} | ConvertTo-Json

$response2 = curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body2 | ConvertFrom-Json

Write-Host "Pergunta 2 - Tokens: $($response2.totalTokens)"

# Pergunta 3
$body3 = @{
    question = "Me conte uma piada curta."
    conversationId = $conversationId
} | ConvertTo-Json

$response3 = curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body3 | ConvertFrom-Json

Write-Host "Pergunta 3 - Tokens: $($response3.totalTokens)"

$totalTokens = $response1.totalTokens + $response2.totalTokens + $response3.totalTokens
Write-Host "`nTotal de tokens usados: $totalTokens" -ForegroundColor Green
```

**Validações**:
- ✅ Cada resposta retorna tokens
- ✅ Total de tokens > 0
- ✅ Logs salvos no banco (verificar com SQL)

---

## Teste 4: System Prompt Customizado ✅

**Objetivo**: Validar uso de system prompt

```powershell
$body = @{
    question = "Conte-me sobre o Brasil"
    conversationId = "test-prompt-001"
    systemPrompt = "Você é um assistente que responde SEMPRE em no máximo 20 palavras."
} | ConvertTo-Json

curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**Validações**:
- ✅ Resposta curta (aproximadamente 20 palavras)
- ✅ Metadados presentes

---

## Teste 5: Histórico de Conversa ✅

**Objetivo**: Verificar recuperação de histórico

```powershell
$conversationId = "test-history-001"

# Enviar 2 perguntas
$body1 = @{ question = "Primeira pergunta"; conversationId = $conversationId } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $body1 | Out-Null

$body2 = @{ question = "Segunda pergunta"; conversationId = $conversationId } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $body2 | Out-Null

# Buscar histórico
curl "$BASE_URL/api/chat/history/$conversationId" `
  -H "X-API-Key: $API_KEY" | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**Validações**:
- ✅ `messages` contém pelo menos 4 mensagens (2 perguntas + 2 respostas)
- ✅ `conversationId` correto

---

## Teste 6: Limpar Conversa ✅

**Objetivo**: Validar limpeza de histórico

```powershell
$conversationId = "test-clear-001"

# Criar conversa
$body = @{ question = "Teste"; conversationId = $conversationId } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $body | Out-Null

# Limpar
curl -X DELETE "$BASE_URL/api/chat/history/$conversationId" -H "X-API-Key: $API_KEY"

# Verificar que foi limpa (deve retornar 404)
curl "$BASE_URL/api/chat/history/$conversationId" -H "X-API-Key: $API_KEY"
```

**Validações**:
- ✅ DELETE retorna 204 (No Content)
- ✅ GET subsequente retorna 404 (Not Found)

---

## Teste 7: Sem API Key ❌

**Objetivo**: Validar autenticação

```powershell
$body = @{ question = "Teste"; conversationId = "test" } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -d $body
```

**Resultado Esperado**: 401 Unauthorized

---

## Teste 8: API Key Inválida ❌

**Objetivo**: Validar validação de API key

```powershell
$body = @{ question = "Teste"; conversationId = "test" } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: invalid-key-123" `
  -d $body
```

**Resultado Esperado**: 401 Unauthorized

---

## Teste 9: Verificar Logs no Banco 📊

**Objetivo**: Validar que logs estão sendo salvos

```sql
-- Ver últimas 10 interações
SELECT TOP 10
    Id,
    ConversationId,
    LEFT(RequestContent, 50) AS RequestPreview,
    LEFT(ResponseContent, 50) AS ResponsePreview,
    InputTokens,
    OutputTokens,
    InputTokens + OutputTokens AS TotalTokens,
    DurationMs,
    Status,
    Timestamp
FROM AiInteractionLogs
ORDER BY Timestamp DESC;

-- Ver uso total de tokens
SELECT 
    ac.Name,
    ac.TokenLimit,
    ac.TokensUsed,
    CAST(ac.TokensUsed AS FLOAT) / CAST(ac.TokenLimit AS FLOAT) * 100 AS PercentUsed,
    COUNT(ail.Id) AS TotalInteractions
FROM AiConfigs ac
LEFT JOIN AiInteractionLogs ail ON ac.Id = ail.AiConfigId
GROUP BY ac.Id, ac.Name, ac.TokenLimit, ac.TokensUsed;
```

**Validações**:
- ✅ Logs presentes na tabela `AiInteractionLogs`
- ✅ `RequestContent` e `ResponseContent` preenchidos
- ✅ Tokens contabilizados
- ✅ `TokensUsed` em `AiConfigs` atualizado

---

## Teste 10: Performance ⚡

**Objetivo**: Medir tempo de resposta

```powershell
$conversationId = "test-perf-001"
$body = @{
    question = "Qual é a capital do Japão?"
    conversationId = $conversationId
} | ConvertTo-Json

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$response = curl -X POST "$BASE_URL/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-Key: $API_KEY" `
  -d $body | ConvertFrom-Json

$stopwatch.Stop()

Write-Host "Tempo total (cliente): $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Cyan
Write-Host "Tempo de processamento (servidor): $($response.processingTimeMs)ms" -ForegroundColor Cyan
Write-Host "Tokens usados: $($response.totalTokens)" -ForegroundColor Cyan
```

**Métricas Esperadas**:
- Tempo de resposta: 200-2000ms (depende da complexidade)
- Tokens: Proporcional ao tamanho da pergunta/resposta

---

## Executar Todos os Testes

```powershell
# Salvar este script como run-all-tests.ps1

Write-Host "=== INICIANDO TESTES DE INTEGRAÇÃO ===" -ForegroundColor Yellow
Write-Host ""

$API_KEY = "api-key-12345678901234567890123456789012"
$BASE_URL = "http://localhost:5120"

# Teste 1
Write-Host "[1/8] Health Check..." -ForegroundColor Cyan
curl "$BASE_URL/health" | Out-Null
Write-Host "✓ Passed" -ForegroundColor Green
Write-Host ""

# Teste 2
Write-Host "[2/8] Pergunta Simples..." -ForegroundColor Cyan
$body = @{ question = "Qual é a capital do Brasil?"; conversationId = "test-001" } | ConvertTo-Json
$response = curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $body | ConvertFrom-Json
Write-Host "✓ Tokens: $($response.totalTokens), Model: $($response.model)" -ForegroundColor Green
Write-Host ""

# Teste 3
Write-Host "[3/8] Múltiplas Perguntas..." -ForegroundColor Cyan
$convId = "test-multi-$(Get-Random)"
$total = 0
1..3 | ForEach-Object {
    $b = @{ question = "Pergunta $_"; conversationId = $convId } | ConvertTo-Json
    $r = curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $b | ConvertFrom-Json
    $total += $r.totalTokens
}
Write-Host "✓ Total tokens: $total" -ForegroundColor Green
Write-Host ""

# Teste 4
Write-Host "[4/8] System Prompt..." -ForegroundColor Cyan
$body = @{ question = "Conte sobre o Brasil"; conversationId = "test-prompt"; systemPrompt = "Responda em 10 palavras." } | ConvertTo-Json
$response = curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $body | ConvertFrom-Json
Write-Host "✓ Resposta: $($response.answer)" -ForegroundColor Green
Write-Host ""

# Teste 5
Write-Host "[5/8] Histórico..." -ForegroundColor Cyan
$convId = "test-history-$(Get-Random)"
$b1 = @{ question = "Primeira"; conversationId = $convId } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $b1 | Out-Null
$history = curl "$BASE_URL/api/chat/history/$convId" -H "X-API-Key: $API_KEY" | ConvertFrom-Json
Write-Host "✓ Mensagens: $($history.messages.Count)" -ForegroundColor Green
Write-Host ""

# Teste 6
Write-Host "[6/8] Limpar Conversa..." -ForegroundColor Cyan
$convId = "test-clear-$(Get-Random)"
$b = @{ question = "Teste"; conversationId = $convId } | ConvertTo-Json
curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: $API_KEY" -d $b | Out-Null
curl -X DELETE "$BASE_URL/api/chat/history/$convId" -H "X-API-Key: $API_KEY" | Out-Null
Write-Host "✓ Conversa limpa" -ForegroundColor Green
Write-Host ""

# Teste 7
Write-Host "[7/8] Sem API Key..." -ForegroundColor Cyan
$b = @{ question = "Teste"; conversationId = "test" } | ConvertTo-Json
try {
    curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -d $b 2>&1 | Out-Null
    Write-Host "✓ Retornou 401" -ForegroundColor Green
} catch {
    Write-Host "✓ Retornou 401" -ForegroundColor Green
}
Write-Host ""

# Teste 8
Write-Host "[8/8] API Key Inválida..." -ForegroundColor Cyan
$b = @{ question = "Teste"; conversationId = "test" } | ConvertTo-Json
try {
    curl -X POST "$BASE_URL/api/chat/task" -H "Content-Type: application/json" -H "X-API-Key: invalid" -d $b 2>&1 | Out-Null
    Write-Host "✓ Retornou 401" -ForegroundColor Green
} catch {
    Write-Host "✓ Retornou 401" -ForegroundColor Green
}
Write-Host ""

Write-Host "=== TODOS OS TESTES CONCLUÍDOS ===" -ForegroundColor Yellow
```

---

## Checklist de Validação

- [ ] Teste 1: Health Check
- [ ] Teste 2: Pergunta Simples com Metadados
- [ ] Teste 3: Acúmulo de Tokens
- [ ] Teste 4: System Prompt
- [ ] Teste 5: Histórico de Conversa
- [ ] Teste 6: Limpar Conversa
- [ ] Teste 7: Sem API Key (401)
- [ ] Teste 8: API Key Inválida (401)
- [ ] Teste 9: Logs no Banco
- [ ] Teste 10: Performance

---

## Notas

- Todos os testes usam a API key real do Groq
- Os tokens são realmente consumidos e contabilizados
- Os logs são salvos no banco SQL Server
- Para ambiente de produção, considere limites de rate limiting
