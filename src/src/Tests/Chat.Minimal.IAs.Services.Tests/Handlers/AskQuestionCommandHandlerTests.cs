using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.Services;
using Moq;
using Xunit;

namespace Chat.Minimal.IAs.Services.Tests.Handlers;

public class AskQuestionCommandHandlerTests
{
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IConversationService> _conversationServiceMock;
    private readonly AskQuestionCommandHandler _handler;

    public AskQuestionCommandHandlerTests()
    {
        _llmServiceMock = new Mock<ILlmService>();
        _conversationServiceMock = new Mock<IConversationService>();
        _handler = new AskQuestionCommandHandler(_llmServiceMock.Object, _conversationServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldOrchestrateFlowCorrectly()
    {
        // Arrange
        var command = new AskQuestionCommand(
            ConversationId: "conv-1",
            Question: "What is 2+2?",
            SystemPrompt: "Math bot"
        );

        var expectedAnswer = "2+2 is 4";

        _llmServiceMock.Setup(x => x.GenerateResponseAsync(
            command.ConversationId,
            command.Question,
            command.SystemPrompt,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnswer);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAnswer, result.Answer);
        Assert.Equal(command.ConversationId, result.ConversationId);
        Assert.Equal(command.Question, result.Question);
        Assert.True(result.ProcessingTimeMs >= 0);

        // Verify calls sequence
        _conversationServiceMock.Verify(x => x.AddQuestionAsync(command.ConversationId, command.Question), Times.Once);
        _llmServiceMock.Verify(x => x.GenerateResponseAsync(command.ConversationId, command.Question, command.SystemPrompt, It.IsAny<CancellationToken>()), Times.Once);
        _conversationServiceMock.Verify(x => x.AddAnswerAsync(command.ConversationId, expectedAnswer), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPropagateException_WhenLlmFails()
    {
        // Arrange
        var command = new AskQuestionCommand("conv-error", "Fatal error");
        _llmServiceMock.Setup(x => x.GenerateResponseAsync(
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception("LLM Down"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));

        // Verify question was added but answer wasn't
        _conversationServiceMock.Verify(x => x.AddQuestionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _conversationServiceMock.Verify(x => x.AddAnswerAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
