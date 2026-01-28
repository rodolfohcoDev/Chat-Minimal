#!/bin/bash
# Script para executar seed de API Key de teste no container MySQL

echo "Executando seed de API Key de teste..."

docker-compose exec mysql mysql -uroot -prootpassword chatminimaldb < scripts/seed-test-apikey.sql

echo "API Key de teste criada com sucesso!"
echo "API Key: test-api-key-12345678901234567890123456789012"
echo "Validade: 2 anos"
echo ""
echo "Use esta API Key nos testes com o header:"
echo "X-API-Key: test-api-key-12345678901234567890123456789012"
