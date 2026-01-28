# Script PowerShell para executar seed de API Key de teste no container MySQL

Write-Host "Executando seed de API Key de teste..." -ForegroundColor Green

Get-Content scripts\seed-test-apikey.sql | docker-compose exec -T mysql mysql -uroot -prootpassword chatminimaldb

Write-Host "`nAPI Key de teste criada com sucesso!" -ForegroundColor Green
Write-Host "API Key: test-api-key-12345678901234567890123456789012" -ForegroundColor Yellow
Write-Host "Validade: 2 anos" -ForegroundColor Yellow
Write-Host ""
Write-Host "Use esta API Key nos testes com o header:" -ForegroundColor Cyan
Write-Host "X-API-Key: test-api-key-12345678901234567890123456789012" -ForegroundColor White
