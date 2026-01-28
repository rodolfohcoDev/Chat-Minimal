using Chat.Minimal.Services.Api.Endpoints;
using Chat.Minimal.Services.Api.Middleware;
using Chat.Minimal.Services.Application.Interfaces;
using Chat.Minimal.Services.Application.Services;
using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Domain.Interfaces;
using Chat.Minimal.Services.Infrastructure.Data;
using Chat.Minimal.Services.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Chat.Minimal.IAs.Services.Extensions; // Adicionar using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(); // Registrar Authorization
builder.Services.AddAuthentication(); // Registrar Authentication

// Configuração do DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=test;User=root;Password=root";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ⭐ Registrar serviços de IA (Class Library)
builder.Services.AddIAServices(builder.Configuration);

// ⭐ Substituir InMemory por Banco de Dados MySQL
builder.Services.AddScoped<Chat.Minimal.IAs.Services.Domain.Interfaces.IConversationMemory, Chat.Minimal.Services.Infrastructure.AI.EfConversationMemory>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat.Minimal.Services API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Use API Key authentication middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// Adicionar Authorization Middleware para suportar .RequireAuthorization()
app.UseAuthentication();
app.UseAuthorization();


// Apply migrations and seed test data in development
// Commented out because migrations are applied manually via dotnet ef

//if (app.Environment.IsDevelopment())
//{
//    using var scope = app.Services.CreateScope();
//    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//    // Apply migrations
////    await context.Database.MigrateAsync();

//    // Seed test API key
//    await Chat.Minimal.Services.Scripts.SeedTestApiKey.SeedAsync(context);
//}

// Map endpoints
app.MapUserEndpoints();
app.MapApiKeyEndpoints();
app.MapChatIAEndpoints(); // ⭐ Endpoints de IA

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();

public partial class Program { } // Necessário para testes de integração
