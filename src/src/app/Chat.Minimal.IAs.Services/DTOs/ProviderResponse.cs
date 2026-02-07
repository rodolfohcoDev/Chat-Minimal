namespace Chat.Minimal.IAs.Services.DTOs;

public class ProviderResponse
{
    public string Content { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
