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
        AiConfig? config = null;
        AiInteractionLog log = new();

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

            // 6. Chamar o serviço de IA
            var response = await _llmService.GenerateResponseAsync(
                conversationId,
                question,
                systemPrompt,
                providerConfig,
                cancellationToken);

            stopwatch.Stop();

            // 6. Estimar tokens (simplificado - em produção usar tokenizer real ou resposta da API)
            var inputTokens = EstimateTokens(question + (systemPrompt ?? ""));
            var outputTokens = EstimateTokens(response);
            var totalTokens = inputTokens + outputTokens;

            // 7. Atualizar uso
            config.TokensUsed += totalTokens;
            config.UpdatedAt = DateTime.UtcNow;

            // 8. Criar log de sucesso
            log = new AiInteractionLog
            {
                AiConfigId = config.Id,
                ConversationId = conversationId,
                UserId = userId,
                RequestContent = requestContent,
                ResponseContent = response,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                Status = "Success",
                Timestamp = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "AI response generated successfully. Config: {ConfigName}, Tokens: {Tokens}, Duration: {Duration}ms",
                config.Name, totalTokens, log.DurationMs);

            return new LlmResponse
            {
                Content = response,
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

            // Log de erro
            if (config != null)
            {
                log = new AiInteractionLog
                {
                    AiConfigId = config.Id,
                    ConversationId = conversationId,
                    UserId = userId,
                    RequestContent = question,
                    ResponseContent = string.Empty,
                    InputTokens = 0,
                    OutputTokens = 0,
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = "Error",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };

                _context.AiInteractionLogs.Add(log);
                await _context.SaveChangesAsync(cancellationToken);
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
