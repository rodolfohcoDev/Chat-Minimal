using Chat.Minimal.IAs.Services.DTOs;

namespace Chat.Minimal.IAs.Services.CQRS.Queries;

public record GetAnswerQuery(string ConversationId, string QuestionId) : IQuery<AnswerDto?>;
