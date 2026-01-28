using Chat.Minimal.IAs.Services.Domain.Entities;

namespace Chat.Minimal.IAs.Services.Domain.Interfaces;

public interface IConversationMemory
{
    Task<Conversation?> GetConversationAsync(string conversationId);
    Task AddMessageAsync(string conversationId, Message message);
    Task ClearConversationAsync(string conversationId);
    Task<bool> ConversationExistsAsync(string conversationId);
}
