using Chat.Minimal.IAs.Services.DTOs;

namespace Chat.Minimal.IAs.Services.CQRS.Commands;

public record AskQuestionCommand(
    string ConversationId,
    string Question,
    string? SystemPrompt = null
) : ICommand<AnswerDto>;
