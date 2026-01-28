$baseUrl = "http://localhost:5000" # Ajuste se necessário ou use argumento
$apiKey = "test-api-key" # Assumindo seed ou key existente

Write-Host "1. Health Check..."
curl "$baseUrl/api/chat/health"

Write-Host "`n2. Perguntar (Ask/Task)..."
curl -X POST "$baseUrl/api/chat/task" `
  -H "Content-Type: application/json" `
  -H "X-API-KEY: $apiKey" `
  -d '{ "question": "Ola, quem e voce?", "conversationId": "ps-test-01" }'

Write-Host "`n3. Historico..."
curl -X GET "$baseUrl/api/chat/history/ps-test-01" `
  -H "X-API-KEY: $apiKey"

Write-Host "`nTestes Concluidos."
