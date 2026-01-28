# Chat.Minimal.Services

API Web ASP.NET Core 10 usando Minimal APIs com arquitetura DDD (Domain-Driven Design), MySQL, Entity Framework Core e Identity Framework.

## 🚀 Tecnologias

- **.NET 10**
- **ASP.NET Core Minimal APIs**
- **Entity Framework Core 9.0**
- **MySQL 8.0** (via Pomelo.EntityFrameworkCore.MySql)
- **ASP.NET Core Identity**
- **Docker & Docker Compose**

## 📁 Estrutura do Projeto (DDD)

```
src/Chat.Minimal.Services/
├── Domain/              # Entidades e interfaces de domínio
│   ├── Entities/
│   └── Interfaces/
├── Infrastructure/      # EF Core, DbContext, Repositórios
│   ├── Data/
│   └── Repositories/
├── Application/         # Serviços, DTOs, Lógica de Negócio
│   ├── DTOs/
│   ├── Services/
│   └── Interfaces/
└── Api/                 # Endpoints Minimal API, Middleware
    ├── Endpoints/
    └── Middleware/
```

## 🔑 Funcionalidades

### Autenticação e Usuários
- Registro de usuários com Identity Framework
- Login de usuários
- CRUD completo de usuários
- Autenticação por API Key

### API Keys
- Geração de API Keys criptograficamente seguras
- Validação de API Keys via header `X-API-Key`
- Listagem de API Keys por usuário
- Revogação de API Keys
- Suporte a data de expiração

## 🛠️ Configuração

### Pré-requisitos
- .NET 10 SDK
- Docker e Docker Compose (opcional)
- MySQL 8.0 (se não usar Docker)

### Configuração do Banco de Dados

Edite `appsettings.json` com suas credenciais MySQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=chatminimaldb;User=root;Password=yourpassword;"
  }
}
```

### Executar com Docker Compose (Recomendado)

```bash
# Iniciar todos os serviços (MySQL + API)
docker-compose up -d

# Visualizar logs
docker-compose logs -f api

# Parar todos os serviços
docker-compose down
```

A API estará disponível em: `http://localhost:8080`

### Executar Localmente

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update --project src/Chat.Minimal.Services

# Executar aplicação
dotnet run --project src/Chat.Minimal.Services
```

A API estará disponível em: `https://localhost:5001` ou `http://localhost:5000`

## 📚 Endpoints da API

### Usuários

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/users/register` | Registrar novo usuário | Não |
| POST | `/api/users/login` | Login de usuário | Não |
| GET | `/api/users/{id}` | Obter usuário por ID | API Key |
| PUT | `/api/users/{id}` | Atualizar usuário | API Key |
| DELETE | `/api/users/{id}` | Deletar usuário | API Key |

### API Keys

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/apikeys` | Gerar nova API Key | API Key |
| GET | `/api/apikeys` | Listar API Keys do usuário | API Key |
| DELETE | `/api/apikeys/{id}` | Revogar API Key | API Key |
| GET | `/api/apikeys/validate?key={key}` | Validar API Key | Não |

### Health Check

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/health` | Verificar status da API |

## 🔐 Autenticação

### 1. Registrar Usuário

```bash
curl -X POST http://localhost:8080/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "userName": "testuser"
  }'
```

### 2. Fazer Login

```bash
curl -X POST http://localhost:8080/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### 3. Gerar API Key

Primeiro, você precisa de uma API Key existente ou criar uma manualmente no banco de dados. Depois:

```bash
curl -X POST http://localhost:8080/api/apikeys \
  -H "Content-Type: application/json" \
  -H "X-API-Key: YOUR_EXISTING_API_KEY" \
  -d '{
    "name": "My API Key",
    "expiresAt": "2025-12-31T23:59:59Z"
  }'
```

### 4. Usar API Key

```bash
curl -X GET http://localhost:8080/api/users/{userId} \
  -H "X-API-Key: YOUR_API_KEY"
```

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar apenas testes unitários
dotnet test Tests/Chat.Minimal.Services.UnitTests

# Executar apenas testes de integração
dotnet test Tests/Chat.Minimal.Services.IntegrationTests
```

## 📖 Documentação OpenAPI

Acesse a documentação interativa da API em:
- **Desenvolvimento**: `http://localhost:8080/openapi/v1.json`

## 🐳 Docker

### Dockerfile

O projeto inclui um Dockerfile multi-estágio otimizado para produção.

### Docker Compose

O `docker-compose.yml` configura:
- **MySQL 8.0**: Banco de dados com persistência de dados
- **API**: Aplicação ASP.NET Core
- **Networking**: Rede bridge para comunicação entre serviços
- **Health Checks**: Garante que o MySQL esteja pronto antes de iniciar a API

## 🔧 Desenvolvimento

### Adicionar Nova Migration

```bash
dotnet ef migrations add MigrationName --project src/Chat.Minimal.Services
```

### Reverter Migration

```bash
dotnet ef migrations remove --project src/Chat.Minimal.Services
```

### Atualizar Banco de Dados

```bash
dotnet ef database update --project src/Chat.Minimal.Services
```

## 📝 Licença

Este projeto é de código aberto e está disponível sob a licença MIT.

## 👥 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

## 📧 Contato

Para dúvidas ou sugestões, entre em contato através das issues do projeto.
