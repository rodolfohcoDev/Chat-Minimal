namespace Chat.Minimal.Services.Api.DTOs.ChatIA;

public record AskQuestionRequest(
    string Question,
    string? ConversationId = null,
    string? SystemPrompt = null
);
