namespace Chat.Minimal.Services.Domain.Entities;

public class AiConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // "Groq", "OpenAI", etc.
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public decimal TokenLimit { get; set; }
    public decimal TokensUsed { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<AiInteractionLog> InteractionLogs { get; set; } = new List<AiInteractionLog>();
}
