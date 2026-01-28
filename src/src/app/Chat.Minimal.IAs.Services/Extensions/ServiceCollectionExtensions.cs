using Chat.Minimal.IAs.Services.Configuration;
using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Dispatchers;
using Chat.Minimal.IAs.Services.CQRS.Handlers;
using Chat.Minimal.IAs.Services.CQRS.Queries;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.IAs.Services.Infrastructure.AI;
using Chat.Minimal.IAs.Services.Infrastructure.Memory;
using Chat.Minimal.IAs.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Minimal.IAs.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIAServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Configurações
        services.Configure<GgufModelSettings>(configuration.GetSection("GgufModelSettings"));
        services.Configure<InferenceSettings>(configuration.GetSection("InferenceSettings"));
        services.Configure<PromptSettings>(configuration.GetSection("PromptSettings"));
        services.Configure<ConversationSettings>(configuration.GetSection("ConversationSettings"));
        services.Configure<AISettings>(configuration.GetSection("AISettings"));

        // 2. Infraestrutura
        services.AddSingleton<IConversationMemory, InMemoryConversationStore>();

        // Registrar implementações concretas
        services.AddSingleton<LlamaSharpService>(); // Singleton (modelo pesado)
        services.AddScoped<LangChainService>();     // Scoped (leve)
        services.AddScoped<OpenAIService>();        // Scoped (leve)

        // Registrar Strategy como implementação principal
        services.AddScoped<ILlmService, GenericLlmService>();

        // 3. Domain Services
        services.AddScoped<IConversationService, ConversationService>();

        // 4. CQRS Dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // 5. Handlers
        services.AddScoped<ICommandHandler<AskQuestionCommand, AnswerDto>, AskQuestionCommandHandler>();
        services.AddScoped<ICommandHandler<ClearConversationCommand>, ClearConversationCommandHandler>();

        services.AddScoped<IQueryHandler<GetConversationHistoryQuery, ConversationDto?>, GetConversationHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetAnswerQuery, AnswerDto?>, GetAnswerQueryHandler>();

        return services;
    }
}
