using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Scripts;

public class SeedTestApiKey
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Migrations are already applied in Program.cs

        // Verificar se já existe a API key de teste
        var existingKey = await context.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == "api-key-12345678901234567890123456789012");

        if (existingKey != null)
        {
            Console.WriteLine("API Key de teste já existe!");
            return;
        }

        // Criar usuário de teste se não existir
        var testUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@chatminimal.com");
        if (testUser == null)
        {
            testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
                Email = "test@chatminimal.com",
                NormalizedEmail = "TEST@CHATMINIMAL.COM",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Usuário de teste criado!");
        }

        // Criar API Key de teste
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Key = "api-key-12345678901234567890123456789012",
            Name = "Test API Key",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(2),
            IsActive = true,
            UserId = testUser.Id
        };

        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ API Key de teste criada com sucesso!");
        Console.WriteLine($"Key: {apiKey.Key}");
        Console.WriteLine($"Expira em: {apiKey.ExpiresAt:yyyy-MM-dd}");
        Console.WriteLine("\nUse esta API Key nos testes com o header:");
        Console.WriteLine($"X-API-Key: {apiKey.Key}");
    }
}
