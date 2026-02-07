using Chat.Minimal.IAs.Services.DTOs;

namespace Chat.Minimal.Services.Application.Interfaces;

public interface IAiOrchestrator
{
    Task<LlmResponse> GenerateResponseAsync(
        string conversationId,
        string question,
        string? userId = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);
}
