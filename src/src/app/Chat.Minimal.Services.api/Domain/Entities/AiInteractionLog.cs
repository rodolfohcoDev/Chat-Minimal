namespace Chat.Minimal.Services.Domain.Entities;

public class AiInteractionLog
{
    public int Id { get; set; }
    public int AiConfigId { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    
    // Conteúdo completo da interação
    public string RequestContent { get; set; } = string.Empty;
    public string ResponseContent { get; set; } = string.Empty;
    
    // Métricas de uso
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
    public double DurationMs { get; set; }
    
    // Status da execução
    public string Status { get; set; } = "Success"; // "Success", "Error"
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; } // HTTP Status Code do provedor
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual AiConfig AiConfig { get; set; } = null!;
}
