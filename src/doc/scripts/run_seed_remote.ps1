Write-Host "Executando temp_seed.sql no banco REMOTO (apolo.hostsrv.org)..."
# Usamos o container 'db' apenas para ter acesso ao cliente 'mysql', mas conectamos no host remoto
Get-Content scripts\temp_seed.sql | docker-compose exec -T db mysql -h apolo.hostsrv.org -P 3306 -u chatuser -pchatpassword!@# chatminimaldb
if ($LASTEXITCODE -eq 0) {
    Write-Host "Seed executado com sucesso no remoto!" -ForegroundColor Green
}
else {
    Write-Host "Erro ao executar seed no remoto." -ForegroundColor Red
}
