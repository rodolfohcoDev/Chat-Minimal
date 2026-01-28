using System.Diagnostics;
using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.IAs.Services.Services;

namespace Chat.Minimal.IAs.Services.CQRS.Handlers;

public class AskQuestionCommandHandler : ICommandHandler<AskQuestionCommand, AnswerDto>
{
    private readonly ILlmService _llmService;
    private readonly IConversationService _conversationService;

    public AskQuestionCommandHandler(
        ILlmService llmService,
        IConversationService conversationService)
    {
        _llmService = llmService;
        _conversationService = conversationService;
    }

    public async Task<AnswerDto> HandleAsync(AskQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // 1. Salvar pergunta
        await _conversationService.AddQuestionAsync(command.ConversationId, command.Question);

        // 2. Gerar resposta
        var answerText = await _llmService.GenerateResponseAsync(
            command.ConversationId,
            command.Question,
            command.SystemPrompt,
            cancellationToken
        );

        // 3. Salvar resposta
        await _conversationService.AddAnswerAsync(command.ConversationId, answerText);

        sw.Stop();

        return new AnswerDto
        {
            AnswerId = Guid.NewGuid().ToString(),
            ConversationId = command.ConversationId,
            Question = command.Question,
            Answer = answerText,
            Timestamp = DateTime.UtcNow,
            ProcessingTimeMs = sw.Elapsed.TotalMilliseconds
        };
    }
}
