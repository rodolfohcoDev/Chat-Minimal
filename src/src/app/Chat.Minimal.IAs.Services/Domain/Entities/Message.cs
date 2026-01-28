namespace Chat.Minimal.IAs.Services.Domain.Entities;

public enum MessageType
{
    System,
    User,
    Assistant
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MessageType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Message() { }

    public Message(MessageType type, string content)
    {
        Type = type;
        Content = content;
    }
}
