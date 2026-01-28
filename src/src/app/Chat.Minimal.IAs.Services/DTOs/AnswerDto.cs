namespace Chat.Minimal.IAs.Services.DTOs;

public class AnswerDto
{
    public string AnswerId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double ProcessingTimeMs { get; set; }
}
