using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;

namespace Chat.Minimal.IAs.Services.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationMemory _memory;

    public ConversationService(IConversationMemory memory)
    {
        _memory = memory;
    }

    public async Task<Conversation?> GetConversationAsync(string conversationId)
    {
        return await _memory.GetConversationAsync(conversationId);
    }

    public async Task AddQuestionAsync(string conversationId, string question)
    {
        var message = new Message(MessageType.User, question);
        await _memory.AddMessageAsync(conversationId, message);
    }

    public async Task AddAnswerAsync(string conversationId, string answer)
    {
        var message = new Message(MessageType.Assistant, answer);
        await _memory.AddMessageAsync(conversationId, message);
    }

    public async Task ClearConversationAsync(string conversationId)
    {
        await _memory.ClearConversationAsync(conversationId);
    }
}
