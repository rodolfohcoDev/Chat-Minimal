using System.Text;
using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Minimal.IAs.Services.Infrastructure.AI;

public class OpenAIService : ILlmService
{
    private readonly AISettings _aiSettings;
    private readonly PromptSettings _promptSettings;
    private readonly IConversationMemory _memory;
    private readonly ILogger<OpenAIService> _logger;
    private readonly OpenAiChatModel _model;

    public OpenAIService(
        IOptions<AISettings> aiSettings,
        IOptions<PromptSettings> promptSettings,
        IConversationMemory memory,
        ILogger<OpenAIService> logger)
    {
        _aiSettings = aiSettings.Value;
        _promptSettings = promptSettings.Value;
        _memory = memory;
        _logger = logger;

        // Validação básica
        if (_aiSettings.Provider == AIProvider.OpenAI && string.IsNullOrEmpty(_aiSettings.OpenAIApiKey))
        {
            _logger.LogWarning("OpenAI API Key não configurada!");
        }

        var provider = new OpenAiProvider(_aiSettings.OpenAIApiKey);
        _model = new OpenAiChatModel(provider, _aiSettings.OpenAIModelName);
    }

    public async Task<string> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conversation = await _memory.GetConversationAsync(conversationId);
            if (conversation == null) conversation = new Conversation(conversationId);
            var messages = BuildMessages(systemPrompt, conversation, question);

            var response = await _model.GenerateAsync(
                new ChatRequest { Messages = messages },
                cancellationToken: cancellationToken);

            return response.Messages.Last().Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar resposta com OpenAI");
            return $"Erro ao conectar com OpenAI: {ex.Message}";
        }
    }

    private List<LangChain.Providers.Message> BuildMessages(
        string? systemPrompt,
        Conversation conversation,
        string currentQuestion)
    {
        var messages = new List<LangChain.Providers.Message>();

        // System Message
        var sysPrompt = systemPrompt ?? _promptSettings.DefaultSystemPrompt;
        if (!string.IsNullOrEmpty(sysPrompt))
        {
            messages.Add(new LangChain.Providers.Message(sysPrompt, MessageRole.System));
        }

        // History
        if (_promptSettings.IncludeHistory && conversation.Messages.Any())
        {
            var history = conversation.Messages
                .TakeLast(_promptSettings.MaxHistoryMessages);

            foreach (var msg in history)
            {
                var role = msg.Type == MessageType.User ? MessageRole.Human : MessageRole.Ai;
                messages.Add(new LangChain.Providers.Message(msg.Content, role));
            }
        }

        // Current Question
        messages.Add(new LangChain.Providers.Message(currentQuestion, MessageRole.Human));

        return messages;
    }
}
