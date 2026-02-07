using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chat.Minimal.IAs.Services.Services;

public class GenericLlmService : ILlmService
{
    private readonly ILlmService _service;

    public GenericLlmService(
        IServiceProvider serviceProvider,
        IOptions<AISettings> settings)
    {
        // Padrão Strategy: Escolhe a implementação baseada na configuração
        switch (settings.Value.Provider)
        {
            case AIProvider.LangChain:
                _service = serviceProvider.GetRequiredService<Chat.Minimal.IAs.Services.Infrastructure.AI.LangChainService>();
                break;
            case AIProvider.OpenAI:
                _service = serviceProvider.GetRequiredService<Chat.Minimal.IAs.Services.Infrastructure.AI.OpenAIService>();
                break;
            case AIProvider.Groq:
                _service = serviceProvider.GetRequiredService<Chat.Minimal.IAs.Services.Infrastructure.AI.GroqService>();
                break;
            case AIProvider.LlamaSharp:
            default:
                _service = serviceProvider.GetRequiredService<Chat.Minimal.IAs.Services.Infrastructure.AI.LlamaSharpService>();
                break;
        }
    }

    public Task<string> GenerateResponseAsync(string conversationId, string question, string? systemPrompt = null, Chat.Minimal.IAs.Services.DTOs.AiProviderConfig? config = null, CancellationToken cancellationToken = default)
    {
        return _service.GenerateResponseAsync(conversationId, question, systemPrompt, config, cancellationToken);
    }
}
