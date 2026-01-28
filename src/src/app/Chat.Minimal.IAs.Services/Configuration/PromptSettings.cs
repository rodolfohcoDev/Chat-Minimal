namespace Chat.Minimal.IAs.Services.Configuration;

public class PromptSettings
{
    public string PromptFormat { get; set; } = "chatml";
    public string DefaultSystemPrompt { get; set; } = "Você é um assistente útil.";

    // Prefixos/Sufixos para formatação manual
    public string UserPrefix { get; set; } = "<|im_start|>user\n";
    public string UserSuffix { get; set; } = "<|im_end|>\n";
    public string AssistantPrefix { get; set; } = "<|im_start|>assistant\n";
    public string AssistantSuffix { get; set; } = "<|im_end|>\n";
    public string SystemPrefix { get; set; } = "<|im_start|>system\n";
    public string SystemSuffix { get; set; } = "<|im_end|>\n";

    public bool IncludeHistory { get; set; } = true;
    public int MaxHistoryMessages { get; set; } = 10;
}
