using System.Collections.Concurrent;
using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Minimal.IAs.Services.Infrastructure.Memory;

public class InMemoryConversationStore : IConversationMemory
{
    private readonly ConcurrentDictionary<string, Conversation> _conversations = new();
    private readonly ConversationSettings _settings;
    private readonly ILogger<InMemoryConversationStore> _logger;
    private readonly Timer? _cleanupTimer;

    public InMemoryConversationStore(
        IOptions<ConversationSettings> settings,
        ILogger<InMemoryConversationStore> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (_settings.AutoCleanup)
        {
            _cleanupTimer = new Timer(CleanupOldConversations, null,
                TimeSpan.FromMinutes(_settings.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(_settings.CleanupIntervalMinutes));
        }
    }

    public Task<Conversation?> GetConversationAsync(string conversationId)
    {
        var conversation = _conversations.GetOrAdd(conversationId, id => new Conversation(id));
        return Task.FromResult<Conversation?>(conversation);
    }

    public Task AddMessageAsync(string conversationId, Message message)
    {
        var conversation = _conversations.GetOrAdd(conversationId, id => new Conversation(id)); // Garante que existe

        lock (conversation) // Thread-safe message addition
        {
            conversation.AddMessage(message);

            // Limit messages per conversation
            if (conversation.Messages.Count > _settings.MaxMessagesPerConversation)
            {
                var removeCount = conversation.Messages.Count - _settings.MaxMessagesPerConversation;
                conversation.Messages.RemoveRange(0, removeCount);
            }
        }

        // Limit total conversations (simple strategy: remove oldest if limit reached)
        if (_conversations.Count > _settings.MaxConversations)
        {
            // Executado em background/async para não bloquear a request atual
            Task.Run(() => EnsureCapacityLimit());
        }

        return Task.CompletedTask;
    }

    public Task ClearConversationAsync(string conversationId)
    {
        _conversations.TryRemove(conversationId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ConversationExistsAsync(string conversationId)
    {
        return Task.FromResult(_conversations.ContainsKey(conversationId));
    }

    private void EnsureCapacityLimit()
    {
        if (_conversations.Count <= _settings.MaxConversations) return;

        var oldest = _conversations.Values
            .OrderBy(c => c.LastActivity)
            .FirstOrDefault();

        if (oldest != null)
        {
            _conversations.TryRemove(oldest.Id, out _);
            _logger.LogInformation("Conversa {ConversationId} removida por limite de capacidade", oldest.Id);
        }
    }

    private void CleanupOldConversations(object? state)
    {
        try
        {
            var timeout = TimeSpan.FromMinutes(_settings.ConversationTimeoutMinutes);
            var now = DateTime.UtcNow;

            var keysToRemove = _conversations.Values
                .Where(c => now - c.LastActivity > timeout)
                .Select(c => c.Id)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _conversations.TryRemove(key, out _);
            }

            if (keysToRemove.Count > 0)
            {
                _logger.LogInformation("Limpeza automática: {Count} conversas inativas removidas", keysToRemove.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante limpeza automática de conversas");
        }
    }
}
