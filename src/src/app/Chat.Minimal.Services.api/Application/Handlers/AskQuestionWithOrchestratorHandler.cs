using System.Diagnostics;
using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.IAs.Services.Services;
using Chat.Minimal.Services.Application.Interfaces;

namespace Chat.Minimal.Services.Application.Handlers;

/// <summary>
/// Handler que usa o AiOrchestrator para validação, logging e controle de uso
/// </summary>
public class AskQuestionWithOrchestratorHandler : ICommandHandler<AskQuestionCommand, AnswerDto>
{
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly IConversationService _conversationService;
    private readonly ILogger<AskQuestionWithOrchestratorHandler> _logger;

    public AskQuestionWithOrchestratorHandler(
        IAiOrchestrator aiOrchestrator,
        IConversationService conversationService,
        ILogger<AskQuestionWithOrchestratorHandler> logger)
    {
        _aiOrchestrator = aiOrchestrator;
        _conversationService = conversationService;
        _logger = logger;
    }

    public async Task<AnswerDto> HandleAsync(AskQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            // 1. Salvar pergunta
            await _conversationService.AddQuestionAsync(command.ConversationId, command.Question);

            // 2. Gerar resposta usando o orquestrador (com validação e logging)
            var llmResponse = await _aiOrchestrator.GenerateResponseAsync(
                command.ConversationId,
                command.Question,
                userId: command.UserId,
                command.SystemPrompt,
                cancellationToken
            );

            // 3. Salvar resposta
            await _conversationService.AddAnswerAsync(command.ConversationId, llmResponse.Content);

            sw.Stop();

            _logger.LogInformation(
                "Question answered successfully. ConversationId: {ConversationId}, Tokens: {Tokens}, Duration: {Duration}ms",
                command.ConversationId, llmResponse.TotalTokens, sw.Elapsed.TotalMilliseconds);

            return new AnswerDto
            {
                AnswerId = Guid.NewGuid().ToString(),
                ConversationId = command.ConversationId,
                Question = command.Question,
                Answer = llmResponse.Content,
                Timestamp = DateTime.UtcNow,
                ProcessingTimeMs = sw.Elapsed.TotalMilliseconds,
                InputTokens = llmResponse.InputTokens,
                OutputTokens = llmResponse.OutputTokens,
                Model = llmResponse.Model,
                Provider = llmResponse.Provider
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, 
                "Error answering question. ConversationId: {ConversationId}, Duration: {Duration}ms",
                command.ConversationId, sw.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
