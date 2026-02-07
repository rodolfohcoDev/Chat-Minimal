using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Scripts;

public static class UpdateGroqApiKey
{
    public static async Task UpdateAsync(ApplicationDbContext context)
    {
        var groqConfig = await context.AiConfigs
            .FirstOrDefaultAsync(c => c.Provider == "Groq");

        if (groqConfig == null)
        {
            Console.WriteLine("⚠ Configuração do Groq não encontrada.");
            return;
        }

        // Atualizar API Key
        groqConfig.ApiKey = "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";
        groqConfig.UpdatedAt = DateTime.UtcNow;
        groqConfig.IsActive = true;

        await context.SaveChangesAsync();

        Console.WriteLine("✓ API Key do Groq atualizada com sucesso!");
        Console.WriteLine($"  - Provider: {groqConfig.Provider}");
        Console.WriteLine($"  - Model: {groqConfig.Model}");
        Console.WriteLine($"  - Updated At: {groqConfig.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
    }
}
