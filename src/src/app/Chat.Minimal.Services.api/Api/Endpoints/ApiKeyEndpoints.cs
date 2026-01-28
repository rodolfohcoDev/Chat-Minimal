using Chat.Minimal.Services.Application.DTOs;
using Chat.Minimal.Services.Application.Interfaces;

namespace Chat.Minimal.Services.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/apikeys").WithTags("API Keys");

        // Generate new API key (protected)
        group.MapPost("/", async (CreateApiKeyDto dto, IApiKeyService apiKeyService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await apiKeyService.GenerateAsync(userId, dto);
            return result != null
                ? Results.Ok(result)
                : Results.BadRequest(new { error = "Failed to generate API key" });
        })
        .WithName("GenerateApiKey");

        // List user's API keys (protected)
        group.MapGet("/", async (IApiKeyService apiKeyService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await apiKeyService.GetUserApiKeysAsync(userId);
            return Results.Ok(result);
        })
        .WithName("GetUserApiKeys");

        // Revoke API key (protected)
        group.MapDelete("/{id:guid}", async (Guid id, IApiKeyService apiKeyService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await apiKeyService.RevokeAsync(id, userId);
            return result
                ? Results.NoContent()
                : Results.NotFound();
        })
        .WithName("RevokeApiKey");

        // Validate API key (public endpoint for testing)
        group.MapGet("/validate", async (string key, IApiKeyService apiKeyService) =>
        {
            var isValid = await apiKeyService.ValidateAsync(key);
            return Results.Ok(new { isValid });
        })
        .WithName("ValidateApiKey");
    }
}
