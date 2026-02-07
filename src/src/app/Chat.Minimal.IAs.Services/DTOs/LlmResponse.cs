namespace Chat.Minimal.IAs.Services.DTOs;

public class LlmResponse
{
    public string Content { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
    public string Model { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
