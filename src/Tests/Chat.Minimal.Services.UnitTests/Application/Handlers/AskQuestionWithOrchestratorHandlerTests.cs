using System;
using System.Threading;
using System.Threading.Tasks;
using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.Services.Application.Handlers;
using Chat.Minimal.Services.Application.Interfaces;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Chat.Minimal.IAs.Services.Services;

namespace Chat.Minimal.Services.UnitTests.Application.Handlers;

public class AskQuestionWithOrchestratorHandlerTests
{
    private readonly Mock<IAiOrchestrator> _mockOrchestrator;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<ILogger<AskQuestionWithOrchestratorHandler>> _mockLogger;
    private readonly AskQuestionWithOrchestratorHandler _handler;

    public AskQuestionWithOrchestratorHandlerTests()
    {
        _mockOrchestrator = new Mock<IAiOrchestrator>();
        _mockConversationService = new Mock<IConversationService>();
        _mockLogger = new Mock<ILogger<AskQuestionWithOrchestratorHandler>>();

        _handler = new AskQuestionWithOrchestratorHandler(
            _mockOrchestrator.Object,
            _mockConversationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldProcessFlowCorrectly()
    {
        // Arrange
        var command = new AskQuestionCommand("conv-1", "Test Question", "System Prompt");

        var llmResponse = new LlmResponse
        {
            Content = "AI Answer",
            InputTokens = 10,
            OutputTokens = 20,
            Model = "gpt-model",
            Provider = "OpenAI"
        };

        _mockOrchestrator
            .Setup(x => x.GenerateResponseAsync(
                command.ConversationId,
                command.Question,
                null, // UserId (assumindo null no command simplificado ou extraído do contexto)
                command.SystemPrompt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResponse);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationId.Should().Be(command.ConversationId);
        result.Question.Should().Be(command.Question);
        result.Answer.Should().Be("AI Answer");
        result.InputTokens.Should().Be(10);
        result.OutputTokens.Should().Be(20);
        result.TotalTokens.Should().Be(30);
        result.Model.Should().Be("gpt-model");
        result.Provider.Should().Be("OpenAI");

        // Verificar interações
        _mockConversationService.Verify(x => x.AddQuestionAsync(command.ConversationId, command.Question), Times.Once);
        _mockConversationService.Verify(x => x.AddAnswerAsync(command.ConversationId, "AI Answer"), Times.Once);
        _mockOrchestrator.Verify(x => x.GenerateResponseAsync(
            command.ConversationId, command.Question, null, command.SystemPrompt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrchestratorFails_ShouldPropagateException()
    {
        // Arrange
        var command = new AskQuestionCommand("1", "Q");
        _mockOrchestrator
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI Error"));

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        // O AddQuestionAsync é chamado ANTES do orquestrador
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("AI Error");
        
        _mockConversationService.Verify(x => x.AddQuestionAsync("1", "Q"), Times.Once);
        // AddAnswerAsync NÃO deve ser chamado se falhar
        _mockConversationService.Verify(x => x.AddAnswerAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
