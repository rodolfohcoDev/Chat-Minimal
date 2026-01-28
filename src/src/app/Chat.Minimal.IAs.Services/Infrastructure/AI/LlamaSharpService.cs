using System.Text;
using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Minimal.IAs.Services.Infrastructure.AI;

public class LlamaSharpService : ILlmService, IDisposable
{
    private readonly LLamaWeights? _model;
    private readonly LLamaContext? _context;
    private readonly GgufModelSettings _config;
    private readonly InferenceSettings _inferenceConfig;
    private readonly PromptSettings _promptConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LlamaSharpService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public LlamaSharpService(
        IOptions<GgufModelSettings> config,
        IOptions<InferenceSettings> inferenceConfig,
        IOptions<PromptSettings> promptConfig,
        IServiceScopeFactory scopeFactory,
        ILogger<LlamaSharpService> logger)
    {
        _config = config.Value;
        _inferenceConfig = inferenceConfig.Value;
        _promptConfig = promptConfig.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;

        try
        {
            if (string.IsNullOrEmpty(_config.ModelPath))
            {
                _logger.LogWarning("Caminho do modelo não configurado.");
                return;
            }

            var modelPath = Path.IsPathRooted(_config.ModelPath)
                ? _config.ModelPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ModelPath);

            if (!File.Exists(modelPath))
            {
                _logger.LogError("Modelo GGUF não encontrado em: {ModelPath}", modelPath);
                return;
            }

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = (uint)_config.ContextSize,
                GpuLayerCount = _config.GpuLayerCount,
                Threads = _config.Threads,
                // Seed foi movido para o pipeline de amostragem na versão 0.17.0

                // UseMmap e UseMlock podem ter mudado na versão 0.10.0 ou dependem de backend específico
                // Removendo temporariamente para fixar build
                // UseMmap = _config.UseMmap,
                // UseMlock = _config.UseMlock
            };

            _logger.LogInformation("Carregando modelo GGUF...");
            _model = LLamaWeights.LoadFromFile(parameters);
            _context = _model.CreateContext(parameters);
            _logger.LogInformation("Modelo carregado com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha crítica ao carregar modelo GGUF");
        }
    }

    public async Task<string> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        if (_model == null || _context == null)
        {
            _logger.LogWarning("Tentativa de inferência sem modelo carregado (Mock Mode)");
            return $"[MOCK] O modelo não foi carregado. Verifique se o arquivo .gguf existe em '{_config.ModelPath}'. Pergunta: {question}";
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var memory = scope.ServiceProvider.GetRequiredService<IConversationMemory>();

            var conversation = await memory.GetConversationAsync(conversationId);
            if (conversation == null) conversation = new Conversation(conversationId);
            var prompt = BuildPrompt(systemPrompt, conversation, question);

            var executor = new InteractiveExecutor(_context);
            var inferenceParams = new InferenceParams
            {
                MaxTokens = _inferenceConfig.MaxTokens,
                AntiPrompts = _inferenceConfig.AntiPrompts
            };

            var responseBuilder = new StringBuilder();

            if (_config.Verbose) _logger.LogDebug("Prompt enviado: {Prompt}", prompt);

            await foreach (var text in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                responseBuilder.Append(text);
            }

            return responseBuilder.ToString().Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante inferência");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private string BuildPrompt(string? systemPrompt, Conversation conversation, string currentQuestion)
    {
        var sb = new StringBuilder();

        sb.Append(_promptConfig.SystemPrefix);
        sb.Append(systemPrompt ?? _promptConfig.DefaultSystemPrompt);
        sb.Append(_promptConfig.SystemSuffix);

        if (_promptConfig.IncludeHistory && conversation.Messages.Any())
        {
            var history = conversation.Messages
                .TakeLast(_promptConfig.MaxHistoryMessages);

            foreach (var msg in history)
            {
                if (msg.Type == MessageType.User)
                {
                    sb.Append(_promptConfig.UserPrefix);
                    sb.Append(msg.Content);
                    sb.Append(_promptConfig.UserSuffix);
                }
                else if (msg.Type == MessageType.Assistant)
                {
                    sb.Append(_promptConfig.AssistantPrefix);
                    sb.Append(msg.Content);
                    sb.Append(_promptConfig.AssistantSuffix);
                }
            }
        }

        sb.Append(_promptConfig.UserPrefix);
        sb.Append(currentQuestion);
        sb.Append(_promptConfig.UserSuffix);

        sb.Append(_promptConfig.AssistantPrefix);

        return sb.ToString();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _model?.Dispose();
        _lock.Dispose();
        _logger.LogInformation("LlamaSharpService descarregado");
    }
}
