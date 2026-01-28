namespace Chat.Minimal.IAs.Services.DTOs;

public class ConversationDto
{
    public string ConversationId { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public DateTime LastActivity { get; set; }
}

public class MessageDto
{
    public string Type { get; set; } = string.Empty; // "User" ou "Assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
