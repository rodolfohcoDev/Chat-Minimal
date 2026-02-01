# Chat.Minimal.Services

API Web ASP.NET Core 10 usando Minimal APIs com arquitetura DDD (Domain-Driven Design), MySQL, Entity Framework Core e Identity Framework.

## 🚀 Tecnologias

- **.NET 10**
- **ASP.NET Core Minimal APIs**
- **Entity Framework Core 9.0**
- **MySQL 8.0** (via Pomelo.EntityFrameworkCore.MySql)
- **ASP.NET Core Identity**
- **Docker & Docker Compose**
- **LLamaSharp / LangChain** (Integração com IA)

## ⚡ Início Rápido

```bash
# 1. Iniciar banco de dados
cd src && docker-compose up -d db

# 2. Aplicar migrations
cd src/app/Chat.Minimal.Services.api
dotnet tool restore
dotnet ef database update

# 3. Criar usuário de teste (opcional)
cd ../../../
docker exec -i chat-minimal-db mysql -uchatuser -p'chatpassword!@#' chatminimaldb < doc/scripts/temp_seed.sql

# 4. Iniciar aplicação
docker-compose up -d
# OU executar localmente: cd src/app/Chat.Minimal.Services.api && dotnet run

# 5. Acessar: http://localhost:5120 (Docker) ou http://localhost:5000 (Local)
```

**API Key de Teste:** `apikey-12345678901234567890123456789012`

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

## 🛠️ Configuração e Execução

### Pré-requisitos
- .NET 9 SDK
- Docker e Docker Compose
- MySQL 8.0 (gerenciado via Docker)

### 🚀 Início Rápido (Recomendado)

#### 1. Iniciar o Banco de Dados MySQL

```bash
cd src
docker-compose up -d db
```

Aguarde alguns segundos para o MySQL inicializar completamente.

#### 2. Aplicar Migrations do Banco de Dados

```bash
cd src/app/Chat.Minimal.Services.api

# Restaurar ferramentas do .NET
dotnet tool restore

# Aplicar migrations
dotnet ef database update
```

Isso criará todas as tabelas necessárias:
- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc. (Identity)
- `ApiKeys` (autenticação)
- `Messages` (histórico de chat)

#### 3. Criar Usuário Base (Opcional)

Execute o script de seed para criar um usuário de teste:

```bash
cd ../../../  # Voltar para o diretório src
docker exec -i chat-minimal-db mysql -uchatuser -p'chatpassword!@#' chatminimaldb < doc/scripts/temp_seed.sql
```

Isso criará:
- **Usuário:** `rodolfohco` (rodolfohco@hotmail.com)
- **API Key:** `apikey-12345678901234567890123456789012`

#### 4. Executar a Aplicação

**Opção A: Via Docker Compose (Produção)**

```bash
cd src
docker-compose up -d
```

A API estará disponível em: `http://localhost:5120`

**Opção B: Localmente (Desenvolvimento)**

```bash
cd src/app/Chat.Minimal.Services.api
dotnet run
```

A API estará disponível em: `http://localhost:5000` (ou confira a porta no console)

#### 5. Acessar a Documentação Swagger

Abra no navegador:
- **Docker:** `http://localhost:5120/`
- **Local:** `http://localhost:5000/`

### 🔧 Configuração Manual

Se preferir configurar manualmente, edite os arquivos de configuração:

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3309;Database=chatminimaldb;User=chatuser;Password=chatpassword!@#"
  }
}
```

**docker-compose.yml:**
- MySQL roda na porta `3309` (mapeada para `3309` no host)
- API roda na porta `5120` (mapeada de `8080` no container)

### 🛑 Parar os Serviços

```bash
cd src
docker-compose down

# Para remover volumes (apaga dados do banco)
docker-compose down -v
```

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

### Usando a API Key de Teste

Se você executou o script de seed (passo 3), já possui uma API Key pronta para uso:

```bash
# Testar autenticação
curl -X GET http://localhost:5120/health \
  -H "X-API-Key: apikey-12345678901234567890123456789012"
```

### 1. Registrar Novo Usuário

```bash
curl -X POST http://localhost:5120/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "novousuario@example.com",
    "password": "Password123!",
    "userName": "novousuario"
  }'
```

### 2. Fazer Login

```bash
curl -X POST http://localhost:5120/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "rodolfohco@hotmail.com",
    "password": "Password123!"
  }'
```

### 3. Gerar Nova API Key

Use a API Key existente para gerar novas:

```bash
curl -X POST http://localhost:5120/api/apikeys \
  -H "Content-Type: application/json" \
  -H "X-API-Key: apikey-12345678901234567890123456789012" \
  -d '{
    "name": "Minha Nova API Key",
    "expiresAt": "2027-12-31T23:59:59Z"
  }'
```

### 4. Listar API Keys do Usuário

```bash
curl -X GET http://localhost:5120/api/apikeys \
  -H "X-API-Key: apikey-12345678901234567890123456789012"
```

### 5. Validar API Key

```bash
curl -X GET "http://localhost:5120/api/apikeys/validate?key=apikey-12345678901234567890123456789012"
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
cd src/app/Chat.Minimal.Services.api
dotnet ef migrations add MigrationName
```

### Reverter Migration

```bash
cd src/app/Chat.Minimal.Services.api
dotnet ef migrations remove
```

### Atualizar Banco de Dados

```bash
cd src/app/Chat.Minimal.Services.api
dotnet ef database update
```

### Gerar Script SQL da Migration

```bash
cd src/app/Chat.Minimal.Services.api
dotnet ef migrations script --output migration.sql
```

## ⚠️ Informações Importantes

### Portas Utilizadas
- **MySQL:** `3309` (host) → `3309` (container)
- **API (Docker):** `5120` (host) → `8080` (container)
- **API (Local):** `5000` ou `5001`

### Credenciais Padrão
- **MySQL Root:** `rootpassword`
- **MySQL User:** `chatuser` / `chatpassword!@#`
- **API Key de Teste:** `apikey-12345678901234567890123456789012`

### Estrutura de Diretórios Atualizada
```
Chat-Minimal/
├── src/
│   ├── src/
│   │   └── app/
│   │       ├── Chat.Minimal.Services.api/    # API principal
│   │       └── Chat.Minimal.IAs.Services/    # Serviços de IA
│   ├── doc/
│   │   └── scripts/                          # Scripts SQL
│   └── docker-compose.yml
└── README.md
```

## 🐛 Troubleshooting

### Erro: "Unable to connect to any of the specified MySQL hosts"
**Solução:** Verifique se o container MySQL está rodando:
```bash
docker ps | grep chat-minimal-db
```

Se não estiver rodando, inicie-o:
```bash
cd src
docker-compose up -d db
```

### Erro: "Access denied for user 'chatuser'"
**Solução:** Recrie o usuário no MySQL:
```bash
docker exec chat-minimal-db mysql -uroot -prootpassword -e "DROP USER IF EXISTS 'chatuser'@'%'; CREATE USER 'chatuser'@'%' IDENTIFIED WITH mysql_native_password BY 'chatpassword!@#'; GRANT ALL PRIVILEGES ON chatminimaldb.* TO 'chatuser'@'%'; FLUSH PRIVILEGES;"
```

### Erro: "dotnet-ef command not found"
**Solução:** Restaure as ferramentas do .NET:
```bash
cd src/app/Chat.Minimal.Services.api
dotnet tool restore
```

### Porta 3309 ou 5120 já em uso
**Solução:** Altere as portas no `docker-compose.yml` ou pare o serviço que está usando a porta.

### Migrations não aplicadas
**Solução:** Verifique se você está no diretório correto e execute:
```bash
cd src/app/Chat.Minimal.Services.api
dotnet ef database update
```

## 📝 Licença

Este projeto é de código aberto e está disponível sob a licença MIT.

## 👥 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

## 📧 Contato

Para dúvidas ou sugestões, entre em contato através das issues do projeto.
