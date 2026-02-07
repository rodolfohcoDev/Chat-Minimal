using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Scripts;

public static class UpdateGroqModel
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

        // Atualizar Modelo para um menor
        groqConfig.Model = "llama-3.1-8b-instant";
        groqConfig.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        Console.WriteLine("✓ Modelo do Groq atualizado com sucesso!");
        Console.WriteLine($"  - Provider: {groqConfig.Provider}");
        Console.WriteLine($"  - Model: {groqConfig.Model}");
        Console.WriteLine($"  - Updated At: {groqConfig.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
    }
}
