using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Scripts;

public static class SeedGroqConfig
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Verificar se já existe configuração
        var existingConfig = await context.AiConfigs.AnyAsync();
        if (existingConfig)
        {
            Console.WriteLine("✓ Configuração de IA já existe no banco de dados.");
            return;
        }

        // Inserir configuração do Groq
        var groqConfig = new AiConfig
        {
            Name = "Groq Default",
            Provider = "Groq",
            Model = "llama-3.3-70b-versatile",
            ApiKey = "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",
            BaseUrl = "https://api.groq.com/openai/v1",
            TokenLimit = 1000000, // 1 milhão de tokens
            TokensUsed = 0,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.AiConfigs.Add(groqConfig);
        await context.SaveChangesAsync();

        Console.WriteLine("✓ Configuração do Groq inserida com sucesso!");
        Console.WriteLine($"  - Provider: {groqConfig.Provider}");
        Console.WriteLine($"  - Model: {groqConfig.Model}");
        Console.WriteLine($"  - Token Limit: {groqConfig.TokenLimit:N0}");
        Console.WriteLine($"  - Expires At: {groqConfig.ExpiresAt:yyyy-MM-dd}");
    }
}
