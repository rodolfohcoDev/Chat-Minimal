namespace Chat.Minimal.IAs.Services.Domain.Interfaces;

public interface ILlmService
{
    Task<string> GenerateResponseAsync(
        string conversationId,
        string question,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);
}
