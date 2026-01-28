namespace Chat.Minimal.IAs.Services.Configuration;

public class ConversationSettings
{
    public int MaxConversations { get; set; } = 100;
    public int MaxMessagesPerConversation { get; set; } = 50;
    public int ConversationTimeoutMinutes { get; set; } = 60;
    public bool AutoCleanup { get; set; } = true;
    public int CleanupIntervalMinutes { get; set; } = 15;
}
