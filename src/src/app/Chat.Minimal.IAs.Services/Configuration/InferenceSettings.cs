namespace Chat.Minimal.IAs.Services.Configuration;

public class InferenceSettings
{
    public float Temperature { get; set; } = 0.7f;
    public int TopK { get; set; } = 40;
    public float TopP { get; set; } = 0.9f;
    public float MinP { get; set; } = 0.05f;
    public float RepeatPenalty { get; set; } = 1.1f;
    public int RepeatLastTokensCount { get; set; } = 64;
    public int MaxTokens { get; set; } = 512;
    public string[] AntiPrompts { get; set; } = ["User:", "Human:", "\n\n\n"];
    public int TimeoutSeconds { get; set; } = 60;
}
