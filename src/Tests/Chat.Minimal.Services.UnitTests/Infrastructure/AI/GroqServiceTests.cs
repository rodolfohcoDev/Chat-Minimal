using System;
using System.Net;
using System.Text.Json;
using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.IAs.Services.Infrastructure.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Chat.Minimal.Services.UnitTests.Infrastructure.AI;

public class GroqServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockFactory;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly Mock<IConversationMemory> _mockMemory;
    private readonly Mock<ILogger<GroqService>> _mockLogger;
    private readonly IOptions<AISettings> _mockSettings;
    private readonly IOptions<PromptSettings> _mockPromptSettings;

    public GroqServiceTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        
        var client = new HttpClient(_mockHandler.Object);
        _mockFactory = new Mock<IHttpClientFactory>();
        _mockFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

        _mockMemory = new Mock<IConversationMemory>();
        _mockLogger = new Mock<ILogger<GroqService>>();
        
        _mockSettings = Options.Create(new AISettings 
        { 
            OpenAIBaseUrl = "https://default-url.com",
            OpenAIApiKey = "default-key",
            OpenAIModelName = "default-model"
        });
        
        _mockPromptSettings = Options.Create(new PromptSettings());
    }

    [Fact]
    public async Task GenerateResponseAsync_WithConfig_ShouldUseConfigValues()
    {
        // Arrange
        var service = new GroqService(
            _mockSettings, 
            _mockPromptSettings, 
            _mockMemory.Object, 
            _mockLogger.Object, 
            _mockFactory.Object
        );

        var config = new AiProviderConfig 
        { 
            ApiKey = "dynamic-key", 
            Model = "dynamic-model", 
            BaseUrl = "https://dynamic-url.com" 
        };

        var responseContent = new 
        { 
            choices = new[] 
            { 
                new { message = new { content = "Dynamic Success" } } 
            } 
        };
        
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Headers.Authorization.Parameter == "dynamic-key" &&
                    req.RequestUri.ToString().StartsWith("https://dynamic-url.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        // Act
        var result = await service.GenerateResponseAsync("conv-1", "test", null, config);

        // Assert
        Assert.Equal("Dynamic Success", result.Content);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithoutConfig_ShouldUseDefaultValues()
    {
        // Arrange
        var service = new GroqService(
            _mockSettings, 
            _mockPromptSettings, 
            _mockMemory.Object, 
            _mockLogger.Object, 
            _mockFactory.Object
        );

        var responseContent = new 
        { 
            choices = new[] 
            { 
                new { message = new { content = "Default Success" } } 
            } 
        };
        
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Headers.Authorization.Parameter == "default-key" && // Should match mockSettings
                    req.RequestUri.ToString().StartsWith("https://default-url.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        // Act
        var result = await service.GenerateResponseAsync("conv-1", "test");

        // Assert
        Assert.Equal("Default Success", result.Content);
    }
}
