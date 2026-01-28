using Chat.Minimal.IAs.Services.DTOs;

namespace Chat.Minimal.IAs.Services.CQRS.Queries;

public record GetConversationHistoryQuery(string ConversationId, int? Limit = null) : IQuery<ConversationDto?>;
