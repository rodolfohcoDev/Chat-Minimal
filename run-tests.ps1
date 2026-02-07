# Script de Testes Automatizados - Chat.Minimal.Services API
# Executa todos os testes de integração e gera relatório

param(
    [string]$BaseUrl = "http://localhost:5120",
    [string]$ApiKey = "api-key-12345678901234567890123456789012"
)

$ErrorActionPreference = "Continue"
$testResults = @()
$totalTests = 0
$passedTests = 0
$failedTests = 0

function Write-TestHeader {
    param([string]$Message)
    Write-Host "`n$Message" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor DarkGray
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Details = ""
    )
    
    $script:totalTests++
    
    if ($Passed) {
        $script:passedTests++
        Write-Host "[PASS] $TestName" -ForegroundColor Green
        $status = "PASSED"
    }
    else {
        $script:failedTests++
        Write-Host "[FAIL] $TestName" -ForegroundColor Red
        $status = "FAILED"
    }
    
    if ($Details) {
        Write-Host "  $Details" -ForegroundColor Gray
    }
    
    $script:testResults += [PSCustomObject]@{
        Test    = $TestName
        Status  = $status
        Details = $Details
    }
}

Write-Host @"
╔═══════════════════════════════════════════════════════════╗
║     CHAT.MINIMAL.SERVICES - INTEGRATION TESTS             ║
║     API Key: Groq (Real)                                  ║
╚═══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Yellow

# Teste 1: Health Check
Write-TestHeader "[1/10] Health Check"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get
    $passed = $response.status -eq "healthy"
    Write-TestResult "Health endpoint responds" $passed "Status: $($response.status)"
}
catch {
    Write-TestResult "Health endpoint responds" $false "Error: $_"
}

# Teste 2: Pergunta Simples
Write-TestHeader "[2/10] Simple Question with Metadata"
try {
    $body = @{
        question       = "Qual é a capital do Brasil?"
        conversationId = "test-simple-$(Get-Random)"
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers
    
    Write-TestResult "Answer received" ($null -ne $response.answer) "Answer length: $($response.answer.Length) chars"
    Write-TestResult "Input tokens tracked" ($response.inputTokens -gt 0) "Input: $($response.inputTokens)"
    Write-TestResult "Output tokens tracked" ($response.outputTokens -gt 0) "Output: $($response.outputTokens)"
    Write-TestResult "Total tokens calculated" ($response.totalTokens -eq ($response.inputTokens + $response.outputTokens)) "Total: $($response.totalTokens)"
    Write-TestResult "Model is correct" ($response.model -eq "llama-3.3-70b-versatile") "Model: $($response.model)"
    Write-TestResult "Provider is correct" ($response.provider -eq "Groq") "Provider: $($response.provider)"
    Write-TestResult "Processing time tracked" ($response.processingTimeMs -gt 0) "Duration: $([math]::Round($response.processingTimeMs, 2))ms"
    
}
catch {
    Write-TestResult "Simple question test" $false "Error: $_"
}

# Teste 3: Múltiplas Perguntas
Write-TestHeader "[3/10] Multiple Questions (Token Accumulation)"
try {
    $conversationId = "test-multi-$(Get-Random)"
    $totalTokens = 0
    $questions = @("Olá!", "Como você está?", "Qual é a capital da França?")
    
    foreach ($question in $questions) {
        $body = @{
            question       = $question
            conversationId = $conversationId
        } | ConvertTo-Json
        
        $headers = @{
            "Content-Type" = "application/json"
            "X-API-Key"    = $ApiKey
        }
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers
        $totalTokens += $response.totalTokens
    }
    
    Write-TestResult "Multiple questions processed" ($totalTokens -gt 0) "Total tokens: $totalTokens across $($questions.Count) questions"
    
}
catch {
    Write-TestResult "Multiple questions test" $false "Error: $_"
}

# Teste 4: System Prompt
Write-TestHeader "[4/10] Custom System Prompt"
try {
    $body = @{
        question       = "Conte-me sobre o Brasil"
        conversationId = "test-prompt-$(Get-Random)"
        systemPrompt   = "Você é um assistente que responde SEMPRE em no máximo 20 palavras."
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers
    $wordCount = ($response.answer -split '\s+').Count
    
    Write-TestResult "System prompt applied" ($wordCount -le 50) "Word count: $wordCount (expected ≤ 50)"
    Write-TestResult "Response received" ($null -ne $response.answer) "Answer: $($response.answer.Substring(0, [Math]::Min(50, $response.answer.Length)))..."
    
}
catch {
    Write-TestResult "System prompt test" $false "Error: $_"
}

# Teste 5: Histórico de Conversa
Write-TestHeader "[5/10] Conversation History"
try {
    $conversationId = "test-history-$(Get-Random)"
    
    # Enviar 2 perguntas
    foreach ($i in 1..2) {
        $body = @{
            question       = "Pergunta $i"
            conversationId = $conversationId
        } | ConvertTo-Json
        
        $headers = @{
            "Content-Type" = "application/json"
            "X-API-Key"    = $ApiKey
        }
        
        Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers | Out-Null
    }
    
    # Buscar histórico
    $headers = @{ "X-API-Key" = $ApiKey }
    $history = Invoke-RestMethod -Uri "$BaseUrl/api/chat/history/$conversationId" -Method Get -Headers $headers
    
    Write-TestResult "History retrieved" ($history.messages.Count -ge 4) "Messages: $($history.messages.Count) (expected ≥ 4)"
    Write-TestResult "Conversation ID matches" ($history.conversationId -eq $conversationId)
    
}
catch {
    Write-TestResult "Conversation history test" $false "Error: $_"
}

# Teste 6: Limpar Conversa
Write-TestHeader "[6/10] Clear Conversation"
try {
    $conversationId = "test-clear-$(Get-Random)"
    
    # Criar conversa
    $body = @{
        question       = "Teste de limpeza"
        conversationId = $conversationId
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers | Out-Null
    
    # Limpar
    $headers = @{ "X-API-Key" = $ApiKey }
    Invoke-RestMethod -Uri "$BaseUrl/api/chat/history/$conversationId" -Method Delete -Headers $headers | Out-Null
    
    # Verificar que foi limpa
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/chat/history/$conversationId" -Method Get -Headers $headers | Out-Null
        Write-TestResult "Conversation cleared" $false "History still exists after delete"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-TestResult "Conversation cleared" $true "Returns 404 as expected"
        }
        else {
            Write-TestResult "Conversation cleared" $false "Unexpected error: $_"
        }
    }
    
}
catch {
    Write-TestResult "Clear conversation test" $false "Error: $_"
}

# Teste 7: Sem API Key
Write-TestHeader "[7/10] No API Key (Security)"
try {
    $body = @{
        question       = "Teste"
        conversationId = "test"
    } | ConvertTo-Json
    
    $headers = @{ "Content-Type" = "application/json" }
    
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers | Out-Null
        Write-TestResult "Rejects request without API key" $false "Request was accepted (should be rejected)"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-TestResult "Rejects request without API key" $true "Returns 401 Unauthorized"
        }
        else {
            Write-TestResult "Rejects request without API key" $false "Unexpected status: $($_.Exception.Response.StatusCode)"
        }
    }
    
}
catch {
    Write-TestResult "No API key test" $false "Error: $_"
}

# Teste 8: API Key Inválida
Write-TestHeader "[8/10] Invalid API Key (Security)"
try {
    $body = @{
        question       = "Teste"
        conversationId = "test"
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = "invalid-key-123"
    }
    
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers | Out-Null
        Write-TestResult "Rejects invalid API key" $false "Request was accepted (should be rejected)"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-TestResult "Rejects invalid API key" $true "Returns 401 Unauthorized"
        }
        else {
            Write-TestResult "Rejects invalid API key" $false "Unexpected status: $($_.Exception.Response.StatusCode)"
        }
    }
    
}
catch {
    Write-TestResult "Invalid API key test" $false "Error: $_"
}

# Teste 9: Performance
Write-TestHeader "[9/10] Performance Measurement"
try {
    $body = @{
        question       = "Qual é a capital do Japão?"
        conversationId = "test-perf-$(Get-Random)"
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body -Headers $headers
    $stopwatch.Stop()
    
    $clientTime = $stopwatch.ElapsedMilliseconds
    $serverTime = $response.processingTimeMs
    
    Write-TestResult "Response time acceptable" ($clientTime -lt 10000) "Client: ${clientTime}ms, Server: $([math]::Round($serverTime, 2))ms"
    Write-TestResult "Tokens tracked" ($response.totalTokens -gt 0) "Tokens: $($response.totalTokens)"
    
}
catch {
    Write-TestResult "Performance test" $false "Error: $_"
}

# Teste 10: Contextual Conversation
Write-TestHeader "[10/10] Contextual Conversation"
try {
    $conversationId = "test-context-$(Get-Random)"
    
    # Primeira mensagem
    $body1 = @{
        question       = "Meu nome é João."
        conversationId = $conversationId
    } | ConvertTo-Json
    
    $headers = @{
        "Content-Type" = "application/json"
        "X-API-Key"    = $ApiKey
    }
    
    $response1 = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body1 -Headers $headers
    
    # Segunda mensagem (deve lembrar o nome)
    $body2 = @{
        question       = "Qual é o meu nome?"
        conversationId = $conversationId
    } | ConvertTo-Json
    
    $response2 = Invoke-RestMethod -Uri "$BaseUrl/api/chat/task" -Method Post -Body $body2 -Headers $headers
    
    $remembersName = $response2.answer -match "João"
    
    Write-TestResult "Maintains conversation context" $remembersName "Response: $($response2.answer.Substring(0, [Math]::Min(100, $response2.answer.Length)))..."
    
}
catch {
    Write-TestResult "Contextual conversation test" $false "Error: $_"
}

# Relatório Final
Write-Host "`n" -NoNewline
Write-Host ("=" * 60) -ForegroundColor Yellow
Write-Host "TEST SUMMARY" -ForegroundColor Yellow
Write-Host ("=" * 60) -ForegroundColor Yellow
Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       $passedTests" -ForegroundColor Green
Write-Host "Failed:       $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })
Write-Host "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Yellow" })
Write-Host ("=" * 60) -ForegroundColor Yellow

# Exportar resultados para JSON
$reportPath = "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$report = @{
    Timestamp   = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    BaseUrl     = $BaseUrl
    TotalTests  = $totalTests
    PassedTests = $passedTests
    FailedTests = $failedTests
    SuccessRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
    Results     = $testResults
} | ConvertTo-Json -Depth 10

$report | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "`nRelatório salvo em: $reportPath" -ForegroundColor Cyan

# Retornar código de saída
if ($failedTests -eq 0) {
    Write-Host "`n✓ ALL TESTS PASSED!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n✗ SOME TESTS FAILED" -ForegroundColor Red
    exit 1
}
