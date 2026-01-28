using Chat.Minimal.IAs.Services.Domain.Entities;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ApiMessage = Chat.Minimal.Services.Domain.Entities.Message;
using LibMessage = Chat.Minimal.IAs.Services.Domain.Entities.Message;

namespace Chat.Minimal.Services.Infrastructure.AI;

public class EfConversationMemory : IConversationMemory
{
    private readonly ApplicationDbContext _context;

    public EfConversationMemory(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddMessageAsync(string conversationId, LibMessage message)
    {
        var dbMessage = new ApiMessage
        {
            ConversationId = conversationId,
            Role = message.Type.ToString().ToLower(), // Enum to string ("user", "assistant")
            Content = message.Content,
            Timestamp = message.Timestamp
        };

        _context.Messages.Add(dbMessage);
        await _context.SaveChangesAsync();
    }

    public async Task<Conversation?> GetConversationAsync(string conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp) // Ordem cronológica
            .ToListAsync();

        if (!messages.Any())
        {
            return null; // ou retornamos nova conversa vazia? InMemory retorna nova. Mas aqui vamos retornar null e let service decidir, ou retornar vazia. InMemory GetOrAdd retorna nova.
                         // O contrato 'GetConversationAsync' deve retornar Conversation?
                         // Se InMemory retorna sempre (GetOrAdd), então aqui também deve retornar Conversation vazia se não existir?
                         // Vamos olhar InMemoryStore... ele usa GetOrAdd.
                         // Se retornar null, o Handler vai quebrar?
                         // Handler GetConversationHistoryQueryHandler verifica null.
                         // ConversationService GetConversationAsync depende disso.

            // Mas AddMessageAsync na Lib chama GetConversationAsync. Se for null?
            // No InMemory, AddMessageAsync chamava GetConversation (sync) que fazia GetOrAdd.
            // Aqui AddMessageAsync não precisa "recuperar" conversa inteira, só insert.

            // Vamos manter GetConversationAsync retornando null se não existir, e o Service lida OU retornamos objeto com lista vazia.
            // Para consistência com InMemory (que sempre retorna), vou retornar objeto.
            return new Conversation(conversationId);
        }

        var conversation = new Conversation(conversationId);
        foreach (var msg in messages)
        {
            // Converter string role para Enum MessageType
            if (Enum.TryParse<MessageType>(msg.Role, true, out var type))
            {
                conversation.AddMessage(new LibMessage(type, msg.Content) { Timestamp = msg.Timestamp });
            }
            else
            {
                // Fallback default
                conversation.AddMessage(new LibMessage(MessageType.System, msg.Content) { Timestamp = msg.Timestamp });
            }
        }

        return conversation;
    }

    public async Task ClearConversationAsync(string conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .ToListAsync();

        if (messages.Any())
        {
            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ConversationExistsAsync(string conversationId)
    {
        return await _context.Messages.AnyAsync(m => m.ConversationId == conversationId);
    }
}
