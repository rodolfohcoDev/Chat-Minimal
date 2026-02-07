namespace Chat.Minimal.IAs.Services.DTOs;

public class AiProviderConfig
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
}
