namespace Chat.Minimal.IAs.Services.Configuration;

public enum AIProvider
{
    LlamaSharp, // Modelo GGUF direto (Local)
    LangChain,  // Via Ollama (Local)
    OpenAI,     // Via API OpenAI (Cloud)
    Groq        // Via API Groq (Cloud - compatível com OpenAI)
}

public class AISettings
{
    public AIProvider Provider { get; set; } = AIProvider.LlamaSharp;

    // Configurações para LangChain/Ollama
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string OllamaModelName { get; set; } = "llama3.2:3b";

    // Configurações para OpenAI
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OpenAIModelName { get; set; } = "gpt-4o-mini"; // Modelo default eficiente
    public string OpenAIOrganization { get; set; } = string.Empty;
    public string OpenAIBaseUrl { get; set; } = "https://api.openai.com/v1"; // Base URL customizável (ex: Groq)
}
