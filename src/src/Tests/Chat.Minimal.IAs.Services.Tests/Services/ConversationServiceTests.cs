using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.Services;
using Moq;
using Xunit;

namespace Chat.Minimal.IAs.Services.Tests.Services;

public class ConversationServiceTests
{
    private readonly Mock<IConversationMemory> _memoryMock;
    private readonly ConversationService _service;

    public ConversationServiceTests()
    {
        _memoryMock = new Mock<IConversationMemory>();
        _service = new ConversationService(_memoryMock.Object);
    }

    [Fact]
    public async Task GetConversation_ShouldCallMemory()
    {
        // Arrange
        var conversationId = "conv-1";
        var expectedConversation = new Conversation(conversationId);
        _memoryMock.Setup(x => x.GetConversationAsync(conversationId))
            .ReturnsAsync(expectedConversation);

        // Act
        var result = await _service.GetConversationAsync(conversationId);

        // Assert
        Assert.Same(expectedConversation, result);
        _memoryMock.Verify(x => x.GetConversationAsync(conversationId), Times.Once);
    }

    [Fact]
    public async Task AddQuestion_ShouldAddUserMessage()
    {
        // Arrange
        var conversationId = "conv-1";
        var question = "Hello?";

        // Act
        await _service.AddQuestionAsync(conversationId, question);

        // Assert
        _memoryMock.Verify(x => x.AddMessageAsync(
            conversationId,
            It.Is<Message>(m => m.Type == MessageType.User && m.Content == question)
        ), Times.Once);
    }

    [Fact]
    public async Task AddAnswer_ShouldAddAssistantMessage()
    {
        // Arrange
        var conversationId = "conv-1";
        var answer = "Hi there!";

        // Act
        await _service.AddAnswerAsync(conversationId, answer);

        // Assert
        _memoryMock.Verify(x => x.AddMessageAsync(
            conversationId,
            It.Is<Message>(m => m.Type == MessageType.Assistant && m.Content == answer)
        ), Times.Once);
    }

    [Fact]
    public async Task ClearConversation_ShouldCallMemory()
    {
        // Arrange
        var conversationId = "conv-1";

        // Act
        await _service.ClearConversationAsync(conversationId);

        // Assert
        _memoryMock.Verify(x => x.ClearConversationAsync(conversationId), Times.Once);
    }
}
