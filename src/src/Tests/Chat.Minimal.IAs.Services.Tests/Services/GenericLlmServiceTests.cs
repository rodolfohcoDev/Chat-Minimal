using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Infrastructure.AI;
using Chat.Minimal.IAs.Services.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chat.Minimal.IAs.Services.Tests.Services;

public class GenericLlmServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public GenericLlmServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    [Fact]
    public void Constructor_ShouldSelectLlamaSharp_WhenProviderIsLlamaSharp()
    {
        // Arrange
        var settings = Options.Create(new AISettings { Provider = AIProvider.LlamaSharp });
        var expectedService = new Mock<LlamaSharpService>(
            Mock.Of<IOptions<GgufModelSettings>>(),
            Mock.Of<IOptions<InferenceSettings>>(),
            Mock.Of<IOptions<PromptSettings>>(),
            Mock.Of<Chat.Minimal.IAs.Services.Domain.Interfaces.IConversationMemory>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<LlamaSharpService>>()
        ).Object;

        _serviceProviderMock.Setup(x => x.GetService(typeof(LlamaSharpService)))
            .Returns(expectedService);

        // Act
        var service = new GenericLlmService(_serviceProviderMock.Object, settings);

        // Assert
        // Reflection to verifying private field if needed, or behavior testing
        // As _service is private, we verify ServiceProvider call
        _serviceProviderMock.Verify(x => x.GetService(typeof(LlamaSharpService)), Times.Once);
        _serviceProviderMock.Verify(x => x.GetService(typeof(LangChainService)), Times.Never);
    }

    [Fact]
    public void Constructor_ShouldSelectLangChain_WhenProviderIsLangChain()
    {
        // Arrange
        var settings = Options.Create(new AISettings { Provider = AIProvider.LangChain });

        // Mock with valid settings to avoid constructor exception
        var validAiSettings = Options.Create(new AISettings { OllamaBaseUrl = "http://localhost:11434" });

        var expectedService = new Mock<LangChainService>(
             validAiSettings,
             Mock.Of<IOptions<PromptSettings>>(),
             Mock.Of<Chat.Minimal.IAs.Services.Domain.Interfaces.IConversationMemory>(),
             Mock.Of<Microsoft.Extensions.Logging.ILogger<LangChainService>>()
        ).Object;

        _serviceProviderMock.Setup(x => x.GetService(typeof(LangChainService)))
            .Returns(expectedService);

        // Act
        var service = new GenericLlmService(_serviceProviderMock.Object, settings);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(LangChainService)), Times.Once);
        _serviceProviderMock.Verify(x => x.GetService(typeof(LlamaSharpService)), Times.Never);
    }

    [Fact]
    public void Constructor_ShouldSelectOpenAI_WhenProviderIsOpenAI()
    {
        // Arrange
        var settings = Options.Create(new AISettings { Provider = AIProvider.OpenAI });

        // Mock with valid settings
        var validAiSettings = Options.Create(new AISettings
        {
            OpenAIApiKey = "sk-valid-key",
            OpenAIModelName = "gpt-model"
        });

        var expectedService = new Mock<OpenAIService>(
             validAiSettings,
             Mock.Of<IOptions<PromptSettings>>(),
             Mock.Of<Chat.Minimal.IAs.Services.Domain.Interfaces.IConversationMemory>(),
             Mock.Of<Microsoft.Extensions.Logging.ILogger<OpenAIService>>()
        ).Object;

        _serviceProviderMock.Setup(x => x.GetService(typeof(OpenAIService)))
            .Returns(expectedService);

        // Act
        var service = new GenericLlmService(_serviceProviderMock.Object, settings);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(OpenAIService)), Times.Once);
    }
}
