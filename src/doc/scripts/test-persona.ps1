$headers = @{
    "Content-Type" = "application/json"
    "X-API-KEY"    = "apikey-12345678901234567890123456789012"
}

$body = @{
    "question"       = "Olá, gostaria de saber quais serviços de IA vocês oferecem?"
    "conversationId" = "test-persona-002"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5120/api/chat/task" -Method Post -Headers $headers -Body $body -ErrorAction Stop
    Write-Host "Resposta da IA (Full JSON):" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Cyan
}
catch {
    Write-Host "Erro na requisição:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errBody = $reader.ReadToEnd()
        Write-Host "Corpo do erro: $errBody" -ForegroundColor DarkRed
    }
}
