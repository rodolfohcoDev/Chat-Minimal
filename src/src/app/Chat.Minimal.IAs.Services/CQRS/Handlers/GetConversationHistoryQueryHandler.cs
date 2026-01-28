using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.CQRS.Queries;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.IAs.Services.Services;

namespace Chat.Minimal.IAs.Services.CQRS.Handlers;

public class GetConversationHistoryQueryHandler : IQueryHandler<GetConversationHistoryQuery, ConversationDto?>
{
    private readonly IConversationService _conversationService;

    public GetConversationHistoryQueryHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<ConversationDto?> HandleAsync(GetConversationHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetConversationAsync(query.ConversationId);

        if (conversation == null) return null;

        var messages = conversation.Messages
            .Select(m => new MessageDto
            {
                Type = m.Type.ToString(),
                Content = m.Content,
                Timestamp = m.Timestamp
            });

        if (query.Limit.HasValue)
        {
            messages = messages.TakeLast(query.Limit.Value);
        }

        return new ConversationDto
        {
            ConversationId = conversation.Id,
            Messages = messages.ToList(),
            TotalMessages = conversation.Messages.Count,
            LastActivity = conversation.LastActivity
        };
    }
}
