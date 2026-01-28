using Chat.Minimal.IAs.Services.Domain.Entities;

namespace Chat.Minimal.IAs.Services.Services;

public interface IConversationService
{
    Task<Conversation?> GetConversationAsync(string conversationId);
    Task AddQuestionAsync(string conversationId, string question);
    Task AddAnswerAsync(string conversationId, string answer);
    Task ClearConversationAsync(string conversationId);
}
