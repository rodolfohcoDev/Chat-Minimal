namespace Chat.Minimal.IAs.Services.Domain.Interfaces;

public interface ILlmService
{
    Task<Chat.Minimal.IAs.Services.DTOs.ProviderResponse> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        Chat.Minimal.IAs.Services.DTOs.AiProviderConfig? config = null,
        CancellationToken cancellationToken = default);
}
