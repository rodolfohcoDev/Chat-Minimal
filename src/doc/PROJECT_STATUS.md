# Status do Projeto - Persistência de Chat

## ✅ Concluído
1. **Entidade Message**: Criada em `Chat.Minimal.Services.Domain.Entities`.
2. **DbContext**: Atualizado com `DbSet<Message>`.
3. **Async Refactoring**: Toda a camada de IA (`IConversationService` etc) foi migrada para Async/Await para melhor performance de IO.
4. **EfConversationMemory**: Implementada persistência no MySQL.
5. **Configuração**: `Program.cs` configurado para usar MySQL para histórico de chat.

## ⚠️ Ação Necessária
Devido a processos em execução travando a build, a Migration do EF Core pode não ter sido criada automaticamente.

### Passos para finalizar:
1. **Pare a aplicação** (se estiver rodando).
2. Execute o comando de migration:
   ```bash
   dotnet ef migrations add AddChatHistory -p src/Chat.Minimal.Services/Chat.Minimal.Services.csproj -s src/Chat.Minimal.Services/Chat.Minimal.Services.csproj
   ```
3. Atualize o banco:
   ```bash
   dotnet ef database update -p src/Chat.Minimal.Services/Chat.Minimal.Services.csproj -s src/Chat.Minimal.Services/Chat.Minimal.Services.csproj
   ```
   OU execute o script SQL manual: `scripts/create_messages_table.sql`.
