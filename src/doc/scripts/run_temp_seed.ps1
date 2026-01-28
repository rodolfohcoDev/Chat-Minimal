Write-Host "Executando temp_seed.sql..."
Get-Content scripts\temp_seed.sql | docker-compose exec -T db mysql -P 3309 -uroot -prootpassword chatminimaldb
if ($LASTEXITCODE -eq 0) {
    Write-Host "Temp seed executado com sucesso!" -ForegroundColor Green
}
else {
    Write-Host "Erro ao executar temp seed." -ForegroundColor Red
}
