namespace Chat.Minimal.IAs.Services.Configuration;

public class GgufModelSettings
{
    public string ModelName { get; set; } = "Llama 3.2 3B Instruct";
    public string ModelPath { get; set; } = "models/llama-3.2-3b-instruct.Q4_K_M.gguf";
    public int ContextSize { get; set; } = 2048;
    public int GpuLayerCount { get; set; } = 0;
    public int Threads { get; set; } = Environment.ProcessorCount / 2;
    public int BatchSize { get; set; } = 512;
    public int Seed { get; set; } = -1;
    public bool UseMmap { get; set; } = true;
    public bool UseMlock { get; set; } = false;
    public bool Verbose { get; set; } = false;
}
