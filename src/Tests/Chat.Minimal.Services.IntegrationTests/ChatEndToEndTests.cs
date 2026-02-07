using System.Net;
using System.Net.Http.Json;
using Chat.Minimal.IAs.Services.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Chat.Minimal.Services.IntegrationTests;

/// <summary>
/// Testes de integração end-to-end para a API de Chat com IA
/// Usa a API key real do Groq para validar toda a cadeia de funcionalidades
/// </summary>
public class ChatEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private const string ApiKey = "api-key-12345678901234567890123456789012";

    public ChatEndToEndTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
    }

    [Fact]
    public async Task Test01_HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
        
        _output.WriteLine($"✓ Health check passed: {content}");
    }

    [Fact]
    public async Task Test02_SimpleQuestion_ShouldReturnAnswerWithMetadata()
    {
        // Arrange
        var conversationId = $"test-simple-{Guid.NewGuid()}";
        var request = new
        {
            question = "Qual é a capital do Brasil?",
            conversationId
        };

        _output.WriteLine($"Sending question: {request.question}");
        _output.WriteLine($"Conversation ID: {conversationId}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/chat/task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AnswerDto>();
        result.Should().NotBeNull();
        
        // Validar campos básicos
        result!.Answer.Should().NotBeNullOrEmpty();
        result.Question.Should().Be(request.question);
        result.ConversationId.Should().Be(conversationId);
        result.AnswerId.Should().NotBeNullOrEmpty();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        
        // Validar metadados de IA
        result.InputTokens.Should().BeGreaterThan(0, "Input tokens should be tracked");
        result.OutputTokens.Should().BeGreaterThan(0, "Output tokens should be tracked");
        result.TotalTokens.Should().Be(result.InputTokens + result.OutputTokens);
        result.Model.Should().Be("llama-3.3-70b-versatile");
        result.Provider.Should().Be("Groq");
        result.ProcessingTimeMs.Should().BeGreaterThan(0);

        // Log dos resultados
        _output.WriteLine($"✓ Answer received: {result.Answer}");
        _output.WriteLine($"✓ Input tokens: {result.InputTokens}");
        _output.WriteLine($"✓ Output tokens: {result.OutputTokens}");
        _output.WriteLine($"✓ Total tokens: {result.TotalTokens}");
        _output.WriteLine($"✓ Processing time: {result.ProcessingTimeMs:F2}ms");
        _output.WriteLine($"✓ Model: {result.Model}");
        _output.WriteLine($"✓ Provider: {result.Provider}");
    }

    [Fact]
    public async Task Test03_ConversationFlow_ShouldMaintainContext()
    {
        // Arrange
        var conversationId = $"test-flow-{Guid.NewGuid()}";
        
        var questions = new[]
        {
            "Meu nome é João.",
            "Qual é o meu nome?"
        };

        _output.WriteLine($"Testing conversation flow with ID: {conversationId}");

        // Act & Assert
        foreach (var question in questions)
        {
            _output.WriteLine($"\nSending: {question}");
            
            var request = new { question, conversationId };
            var response = await _client.PostAsJsonAsync("/api/chat/task", request);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<AnswerDto>();
            result.Should().NotBeNull();
            result!.Answer.Should().NotBeNullOrEmpty();
            
            _output.WriteLine($"Received: {result.Answer}");
            _output.WriteLine($"Tokens: {result.TotalTokens} ({result.InputTokens} in + {result.OutputTokens} out)");
        }

        // Verificar histórico
        var historyResponse = await _client.GetAsync($"/api/chat/history/{conversationId}");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var history = await historyResponse.Content.ReadFromJsonAsync<ConversationDto>();
        history.Should().NotBeNull();
        history!.Messages.Should().HaveCountGreaterThanOrEqualTo(4); // 2 perguntas + 2 respostas
        
        _output.WriteLine($"\n✓ Conversation history has {history.Messages.Count} messages");
    }

    [Fact]
    public async Task Test04_MultipleQuestions_ShouldAccumulateTokens()
    {
        // Arrange
        var conversationId = $"test-tokens-{Guid.NewGuid()}";
        var questions = new[]
        {
            "Olá!",
            "Como você está?",
            "Qual é a capital da França?"
        };

        var totalTokens = 0;
        var results = new List<AnswerDto>();

        _output.WriteLine($"Testing token accumulation with {questions.Length} questions");

        // Act
        foreach (var question in questions)
        {
            var request = new { question, conversationId };
            var response = await _client.PostAsJsonAsync("/api/chat/task", request);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<AnswerDto>();
            result.Should().NotBeNull();
            
            results.Add(result!);
            totalTokens += result!.TotalTokens;
            
            _output.WriteLine($"Q{results.Count}: {question}");
            _output.WriteLine($"   Tokens: {result.TotalTokens} (cumulative: {totalTokens})");
        }

        // Assert
        totalTokens.Should().BeGreaterThan(0);
        results.Should().HaveCount(questions.Length);
        results.Should().OnlyContain(r => r.Provider == "Groq");
        results.Should().OnlyContain(r => r.Model == "llama-3.3-70b-versatile");

        _output.WriteLine($"\n✓ Total tokens used: {totalTokens}");
    }

    [Fact]
    public async Task Test05_WithSystemPrompt_ShouldFollowInstructions()
    {
        // Arrange
        var conversationId = $"test-prompt-{Guid.NewGuid()}";
        var request = new
        {
            question = "Conte-me sobre o Brasil",
            conversationId,
            systemPrompt = "Você é um assistente que responde SEMPRE em no máximo 20 palavras, de forma muito concisa."
        };

        _output.WriteLine($"Testing with system prompt: {request.systemPrompt}");
        _output.WriteLine($"Question: {request.question}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/chat/task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AnswerDto>();
        result.Should().NotBeNull();
        result!.Answer.Should().NotBeNullOrEmpty();
        
        var wordCount = result.Answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        
        _output.WriteLine($"Answer: {result.Answer}");
        _output.WriteLine($"Word count: {wordCount}");
        _output.WriteLine($"Tokens: {result.TotalTokens}");
        
        // A resposta deve ser relativamente curta devido ao system prompt
        wordCount.Should().BeLessThanOrEqualTo(50, "System prompt should influence response length");
    }

    [Fact]
    public async Task Test06_ClearConversation_ShouldRemoveHistory()
    {
        // Arrange
        var conversationId = $"test-clear-{Guid.NewGuid()}";
        
        // Criar conversa
        var request = new { question = "Teste de limpeza", conversationId };
        await _client.PostAsJsonAsync("/api/chat/task", request);

        _output.WriteLine($"Created conversation: {conversationId}");

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/chat/history/{conversationId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar que foi limpa
        var getResponse = await _client.GetAsync($"/api/chat/history/{conversationId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        _output.WriteLine($"✓ Conversation cleared successfully");
    }

    [Fact]
    public async Task Test07_WithoutApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var clientWithoutKey = _factory.CreateClient();
        var request = new { question = "Teste", conversationId = "test" };

        _output.WriteLine("Testing request without API key");

        // Act
        var response = await clientWithoutKey.PostAsJsonAsync("/api/chat/task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        _output.WriteLine($"✓ Correctly returned {response.StatusCode}");
    }

    [Fact]
    public async Task Test08_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var clientWithInvalidKey = _factory.CreateClient();
        clientWithInvalidKey.DefaultRequestHeaders.Add("X-API-Key", "invalid-key-123");
        var request = new { question = "Teste", conversationId = "test" };

        _output.WriteLine("Testing request with invalid API key");

        // Act
        var response = await clientWithInvalidKey.PostAsJsonAsync("/api/chat/task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        _output.WriteLine($"✓ Correctly returned {response.StatusCode}");
    }
}
