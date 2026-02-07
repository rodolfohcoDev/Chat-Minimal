using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Minimal.IAs.Services.Infrastructure.AI;

public class GroqService : ILlmService
{
    private readonly AISettings _aiSettings;
    private readonly PromptSettings _promptSettings;
    private readonly IConversationMemory _memory;
    private readonly ILogger<GroqService> _logger;
    private readonly HttpClient _httpClient;

    public GroqService(
        IOptions<AISettings> aiSettings,
        IOptions<PromptSettings> promptSettings,
        IConversationMemory memory,
        ILogger<GroqService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _aiSettings = aiSettings.Value;
        _promptSettings = promptSettings.Value;
        _memory = memory;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        // Não definir BaseAddress, usar URL completa no PostAsync
        // _httpClient.DefaultRequestHeaders.Authorization será definido por request
    }

    public async Task<string> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        Chat.Minimal.IAs.Services.DTOs.AiProviderConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = config?.ApiKey ?? _aiSettings.OpenAIApiKey;
            var model = config?.Model ?? _aiSettings.OpenAIModelName;
            var baseUrl = config?.BaseUrl ?? _aiSettings.OpenAIBaseUrl;

            var conversation = await _memory.GetConversationAsync(conversationId);
            if (conversation == null) conversation = new Conversation(conversationId);

            var messages = BuildMessages(systemPrompt, conversation, question);

            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 512
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var url = $"{baseUrl}/chat/completions";
            _logger.LogInformation("Groq API URL: {Url}", url);
            
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = content;
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonDocument.Parse(responseBody);

            var answer = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return answer ?? "Sem resposta";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar resposta com Groq");
            return $"Erro ao conectar com Groq: {ex.Message}";
        }
    }

    private List<object> BuildMessages(
        string? systemPrompt,
        Conversation conversation,
        string currentQuestion)
    {
        var messages = new List<object>();

        // System Message
        var sysPrompt = systemPrompt ?? _promptSettings.DefaultSystemPrompt;
        if (!string.IsNullOrEmpty(sysPrompt))
        {
            messages.Add(new { role = "system", content = sysPrompt });
        }

        // History
        if (_promptSettings.IncludeHistory && conversation.Messages.Any())
        {
            var history = conversation.Messages
                .TakeLast(_promptSettings.MaxHistoryMessages);

            foreach (var msg in history)
            {
                var role = msg.Type == MessageType.User ? "user" : "assistant";
                messages.Add(new { role, content = msg.Content });
            }
        }

        // Current Question
        messages.Add(new { role = "user", content = currentQuestion });

        return messages;
    }
}
