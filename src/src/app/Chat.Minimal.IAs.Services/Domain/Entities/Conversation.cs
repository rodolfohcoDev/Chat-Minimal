namespace Chat.Minimal.IAs.Services.Domain.Entities;

public class Conversation
{
    public string Id { get; set; }
    public List<Message> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public Conversation(string id)
    {
        Id = id;
    }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
        LastActivity = DateTime.UtcNow;
    }
}
