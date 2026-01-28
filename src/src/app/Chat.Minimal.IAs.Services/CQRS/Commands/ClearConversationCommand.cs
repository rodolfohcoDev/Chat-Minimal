namespace Chat.Minimal.IAs.Services.CQRS.Commands;

public record ClearConversationCommand(string ConversationId) : ICommand;
