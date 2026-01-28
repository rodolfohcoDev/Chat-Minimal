using Chat.Minimal.IAs.Services.CQRS.Commands;
using Chat.Minimal.IAs.Services.CQRS.Dispatchers;
using Chat.Minimal.IAs.Services.CQRS.Queries;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.Services.Api.DTOs.ChatIA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Chat.Minimal.Services.Api.Endpoints;

public static class ChatIAEndpoints
{
    public static void MapChatIAEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat IA")
            .RequireAuthorization(); // Requer autenticação de API Key ou JWT existente

        // POST /api/chat/task
        group.MapPost("/task", async (
            [FromBody] AskQuestionRequest request,
            ICommandDispatcher commandDispatcher,
            HttpContext context) =>
        {
            // Opcional: Pegar ID do usuário logado se necessário
            // var userId = context.User.Identity?.Name;

            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();

            var command = new AskQuestionCommand(
                ConversationId: conversationId,
                Question: request.Question,
                SystemPrompt: request.SystemPrompt
            );

            try
            {
                var result = await commandDispatcher.DispatchAsync<AskQuestionCommand, AnswerDto>(command);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("AskQuestionTask")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Faz uma pergunta para a IA",
            Description = "Envia uma pergunta para o serviço de IA configurado (LlamaSharp, LangChain ou OpenAI) e retorna a resposta."
        });

        // GET /api/chat/history/{conversationId}
        group.MapGet("/history/{conversationId}", async (
            string conversationId,
            IQueryDispatcher queryDispatcher,
            [FromQuery] int? limit) =>
        {
            var query = new GetConversationHistoryQuery(conversationId, limit);
            var result = await queryDispatcher.DispatchAsync<GetConversationHistoryQuery, ConversationDto?>(query);

            if (result == null)
            {
                return Results.NotFound(new { message = "Conversa não encontrada" });
            }

            return Results.Ok(result);
        })
        .WithName("GetConversationHistory")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Obtém o histórico da conversa",
            Description = "Retorna as mensagens anteriores de uma conversa específica."
        });

        // DELETE /api/chat/history/{conversationId}
        group.MapDelete("/history/{conversationId}", async (
            string conversationId,
            ICommandDispatcher commandDispatcher) =>
        {
            var command = new ClearConversationCommand(conversationId);
            await commandDispatcher.DispatchAsync(command);
            return Results.NoContent();
        })
        .WithName("ClearConversation")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Limpa o histórico da conversa",
            Description = "Remove todas as mensagens de uma conversa da memória."
        });
    }
}
