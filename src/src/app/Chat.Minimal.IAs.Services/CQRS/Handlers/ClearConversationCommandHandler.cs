using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.Services;

namespace Chat.Minimal.IAs.Services.CQRS.Handlers;

public class ClearConversationCommandHandler : ICommandHandler<ClearConversationCommand>
{
    private readonly IConversationService _conversationService;

    public ClearConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task HandleAsync(ClearConversationCommand command, CancellationToken cancellationToken = default)
    {
        await _conversationService.ClearConversationAsync(command.ConversationId);
    }
}
