using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.UnitTests.Infrastructure.Data;

public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Converter tipos incompatíveis com SQLite
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnType()?.ToUpper().Contains("NVARCHAR(MAX)") == true)
                {
                    property.SetColumnType("TEXT");
                }
                
                // Decimal com precisão não é suportado nativamente, converter para double ou TEXT
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("TEXT"); 
                }
            }
        }
    }
}
