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

    public async Task<Chat.Minimal.IAs.Services.DTOs.ProviderResponse> GenerateResponseAsync(
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
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                 // Log de falha detalhado
                 _logger.LogWarning("Groq API Error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                 
                 return new Chat.Minimal.IAs.Services.DTOs.ProviderResponse
                 {
                     IsSuccess = false,
                     StatusCode = (int)response.StatusCode,
                     Content = responseBody, // Retorna o JSON de erro do provedor
                     ErrorMessage = $"HTTP {response.StatusCode} {response.ReasonPhrase}"
                 };
            }

            var jsonResponse = JsonDocument.Parse(responseBody);

            var answer = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Tentar extrair tokens
            int inputTokens = 0, outputTokens = 0;
            if (jsonResponse.RootElement.TryGetProperty("usage", out var usage))
            {
               if (usage.TryGetProperty("prompt_tokens", out var pt)) inputTokens = pt.GetInt32();
               if (usage.TryGetProperty("completion_tokens", out var ct)) outputTokens = ct.GetInt32();
            }

            return new Chat.Minimal.IAs.Services.DTOs.ProviderResponse
            {
                IsSuccess = true,
                StatusCode = (int)response.StatusCode,
                Content = answer ?? "Sem resposta",
                InputTokens = inputTokens,
                OutputTokens = outputTokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar resposta com Groq");
            return new Chat.Minimal.IAs.Services.DTOs.ProviderResponse
            {
                IsSuccess = false,
                StatusCode = 500,
                ErrorMessage = ex.Message,
                Content = ex.ToString()
            };
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
