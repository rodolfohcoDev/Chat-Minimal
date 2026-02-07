# Testes Unitários e Cobertura

Este projeto inclui uma suíte abrangente de testes unitários localizados em `src/Tests/Chat.Minimal.Services.UnitTests`.

## Cobertura

Os testes cobrem os componentes críticos do sistema com foco em:

1.  **AiOrchestrator**: Lógica de negócios central.
    - Validação de configurações ativas.
    - Verificação de expiração e limites de tokens.
    - Geração de logs de sucesso e erro.
    - Passagem correta de credenciais dinâmicas para os serviços de IA.
    
2.  **GroqService**: Integração com o provedor de IA.
    - Uso correto de configurações dinâmicas (API Key do banco).
    - Fallback para configurações estáticas (appsettings).
    - Tratamento de erros de comunicação HTTP.

## Tecnologias Utilizadas

- **xUnit**: Framework de testes.
- **Moq**: Mocking de dependências (ILlmService, ILogger, IHttpClientFactory).
- **FluentAssertions**: Asserções legíveis.
- **SQLite In-Memory**: Simulação de banco de dados relacional para testes de integração de componentes EF Core, permitindo testar consultas reais e restrições de chave estrangeira sem um banco SQL Server.

## Como Executar

Para rodar todos os testes unitários:

```bash
dotnet test src/Tests/Chat.Minimal.Services.UnitTests
```

## Estrutura de Testes

- `Application/Services/AiOrchestratorTests.cs`: Testes isolados do orquestrador.
- `Infrastructure/AI/GroqServiceTests.cs`: Testes do serviço Groq com Mock HTTP.
- `Infrastructure/Data/TestApplicationDbContext.cs`: Contexto de banco de dados específico para testes que adapta tipos SQL Server para SQLite.

## Scripts de Dados

Para criar usuários e chaves de API para consumo, utilize o script gerado:
- `src/src/app/Chat.Minimal.Services.api/Scripts/CreateConsumerApiKey.sql`
