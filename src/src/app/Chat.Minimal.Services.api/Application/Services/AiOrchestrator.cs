using System.Diagnostics;
using System.Text.Json;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.Services.Application.Interfaces;
using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Application.Services;

public class AiOrchestrator : IAiOrchestrator
{
    private readonly ILlmService _llmService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AiOrchestrator> _logger;

    public AiOrchestrator(
        ILlmService llmService,
        ApplicationDbContext context,
        ILogger<AiOrchestrator> logger)
    {
        _llmService = llmService;
        _context = context;
        _logger = logger;
    }

    public async Task<LlmResponse> GenerateResponseAsync(
        string conversationId,
        string question,
        string? userId = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Chat.Minimal.Services.Domain.Entities.AiConfig? config = null;

        try
        {
            // 1. Buscar configuração ativa
            config = await _context.AiConfigs
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (config == null)
            {
                throw new InvalidOperationException("Nenhuma configuração de IA ativa encontrada.");
            }

            // 2. Validar expiração
            if (config.ExpiresAt.HasValue && config.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException($"A configuração de IA '{config.Name}' expirou em {config.ExpiresAt.Value:yyyy-MM-dd HH:mm:ss}.");
            }

            // 3. Validar limite de tokens
            if (config.TokensUsed >= config.TokenLimit)
            {
                throw new InvalidOperationException($"Limite de tokens atingido para a configuração '{config.Name}'. Usado: {config.TokensUsed}/{config.TokenLimit}.");
            }

            // 4. Preparar request content para log
            var requestData = new
            {
                conversationId,
                question,
                systemPrompt,
                timestamp = DateTime.UtcNow
            };
            var requestContent = JsonSerializer.Serialize(requestData);

            // 5. Preparar configuração do provedor
            var providerConfig = new Chat.Minimal.IAs.Services.DTOs.AiProviderConfig
            {
                Provider = config.Provider,
                Model = config.Model,
                ApiKey = config.ApiKey,
                BaseUrl = config.BaseUrl
            };

            // 6. Chamar o serviço de IA (Agora retorna ProviderResponse)
            var response = await _llmService.GenerateResponseAsync(
                conversationId,
                question,
                systemPrompt,
                providerConfig,
                cancellationToken);

            stopwatch.Stop();

            // 7. Calcular tokens
            var inputTokens = response.InputTokens > 0 ? response.InputTokens : EstimateTokens(question + (systemPrompt ?? ""));
            var outputTokens = response.OutputTokens > 0 ? response.OutputTokens : EstimateTokens(response.Content);
            var totalTokens = inputTokens + outputTokens;

            // 8. Atualizar uso (apenas se sucesso)
            if (response.IsSuccess)
            {
                config.TokensUsed += totalTokens;
                config.UpdatedAt = DateTime.UtcNow;
            }

            // 9. Criar log (Sucesso ou Falha do Provedor)
            var log = new Chat.Minimal.Services.Domain.Entities.AiInteractionLog
            {
                AiConfigId = config.Id,
                ConversationId = conversationId,
                UserId = userId,
                RequestContent = requestContent,
                ResponseContent = response.Content, // Salva resposta ou erro JSON do provedor
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                Status = response.IsSuccess ? "Success" : "Error",
                StatusCode = response.StatusCode, // Novo campo
                ErrorMessage = response.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            if (!response.IsSuccess)
            {
                // Lança exceção para o handler, mas já logou no banco
                throw new InvalidOperationException($"Erro no provedor de IA ({response.StatusCode}): {response.ErrorMessage}");
            }

            _logger.LogInformation(
                "AI response generated successfully. Config: {ConfigName}, Tokens: {Tokens}, Duration: {Duration}ms",
                config.Name, totalTokens, log.DurationMs);

            return new LlmResponse
            {
                Content = response.Content,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                Model = config.Model,
                Provider = config.Provider
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error generating AI response for conversation {ConversationId}", conversationId);

            // Log de erro para falhas ANTES de chamar o provedor (ex: validação, banco de dados)
            // Se log já foi salvo acima (no fluxo de provider error), não salva de novo se ex for a que lançamos
            // Mas como estamos dentro do try, se lançarmos exceção, cai aqui? Sim.

            // Verificar se é uma exceção que já tratamos e logamos (InvalidOperationException do provider)
            // Se for do provider, já salvamos o log.
            // Mas InvalidOperationException é generica.

            // Melhor abordagem: se a falha ocorrer antes de chamar o serviço, config pode ser nulo.
            if (config != null && !ex.Message.StartsWith("Erro no provedor de IA")) 
            {
                // Verifica se já não existe tracking? Não dá pra saber facil com EF tracking sem checkar Local
                // Simplesmente adicionamos um log de "System Error"
                var errorLog = new Chat.Minimal.Services.Domain.Entities.AiInteractionLog
                {
                    AiConfigId = config.Id,
                    ConversationId = conversationId,
                    UserId = userId,
                    RequestContent = question,
                    ResponseContent = string.Empty,
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = "SystemError",
                    StatusCode = 500,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
                 _context.AiInteractionLogs.Add(errorLog);
                 // Ignorar erro ao salvar log de erro
                 try { await _context.SaveChangesAsync(cancellationToken); } catch { }
            }

            throw;
        }
    }

    private int EstimateTokens(string text)
    {
        // Estimativa simples: ~4 caracteres por token (média para português/inglês)
        // Em produção, usar um tokenizer real como tiktoken
        return (int)Math.Ceiling(text.Length / 4.0);
    }
}
