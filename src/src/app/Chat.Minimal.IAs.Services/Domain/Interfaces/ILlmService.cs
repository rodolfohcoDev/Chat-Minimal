namespace Chat.Minimal.IAs.Services.Domain.Interfaces;

public interface ILlmService
{
    Task<string> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        Chat.Minimal.IAs.Services.DTOs.AiProviderConfig? config = null,
        CancellationToken cancellationToken = default);
}
