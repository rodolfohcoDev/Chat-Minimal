# 🐳 Guia de Publicação Docker

## ✅ Status
A aplicação foi publicada com sucesso no Docker!

## 📦 Containers Rodando
- **chat-minimal-api**: API .NET 9 na porta `8080`
- **chat-minimal-db**: MySQL 8.0 na porta `3309` (host) → `3306` (container)

## 🚀 Como Usar

### Iniciar os containers
```bash
docker-compose up -d
```

### Parar os containers
```bash
docker-compose down
```

### Parar e remover volumes (limpar banco)
```bash
docker-compose down -v
```

### Ver logs
```bash
# Logs da API
docker logs chat-minimal-api -f

# Logs do MySQL
docker logs chat-minimal-db -f
```

### Rebuild da imagem
```bash
docker build -t chat-minimal-api:latest .
docker-compose up -d --force-recreate
```

## 🔗 Endpoints Disponíveis
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080 (em Development)
- **Health Check**: http://localhost:8080/health
- **MySQL**: localhost:3309

## 🧪 Testar a API

```bash
# Health Check
curl http://localhost:8080/health

# Fazer pergunta (requer API Key válida no banco)
curl --location 'http://localhost:8080/api/chat/task' \
--header 'Content-Type: application/json' \
--header 'X-API-KEY: sua-api-key-aqui' \
--data '{
    "question": "Olá, como você está?",
    "conversationId": "docker-test-01"
}'
```

## ⚙️ Configurações

### Variáveis de Ambiente (docker-compose.yml)
- `ASPNETCORE_ENVIRONMENT`: Production
- `ConnectionStrings__DefaultConnection`: String de conexão do MySQL
- `GgufModelSettings__ModelPath`: Caminho do modelo GGUF (se usar LlamaSharp)

### Provider de IA
Por padrão, está configurado para usar **LangChain + Ollama**.
Para usar Ollama, certifique-se de que ele está rodando em `http://host.docker.internal:11434`.

## 📝 Notas
- O banco de dados é persistido em um volume Docker (`chat_micro_mysql_data`)
- As migrations são aplicadas automaticamente na inicialização (se configurado)
- Para usar modelos GGUF locais, monte o volume: `./models:/app/models`

## 🔧 Troubleshooting

### Porta já em uso
Se a porta 8080 ou 3309 estiver em uso, altere no `docker-compose.yml`:
```yaml
ports:
  - "NOVA_PORTA:8080"  # Para a API
  - "NOVA_PORTA:3306"  # Para o MySQL
```

### Erro de conexão com o banco
Aguarde o healthcheck do MySQL (~10-15 segundos após `docker-compose up`).

### Modelo GGUF não encontrado
Use o provider LangChain/Ollama ou monte o volume com o modelo:
```yaml
volumes:
  - ./models:/app/models
```
