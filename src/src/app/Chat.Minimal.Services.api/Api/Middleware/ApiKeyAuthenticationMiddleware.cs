using Chat.Minimal.Services.Application.Interfaces;

namespace Chat.Minimal.Services.Api.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Skip API key validation for certain endpoints
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (path.Contains("/api/users/register") ||
            path.Contains("/api/users/login") ||
            path.Contains("/api/apikeys/validate") ||
            path.Contains("/swagger") ||
            path.Contains("/health"))
        {
            await _next(context);
            return;
        }

        // Check if API key header exists
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is missing" });
            return;
        }

        // Validate API key
        var isValid = await apiKeyService.ValidateAsync(extractedApiKey!);
        if (!isValid)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired API Key" });
            return;
        }

        // Get user ID from API key and add to context
        var userId = await apiKeyService.GetUserIdByApiKeyAsync(extractedApiKey!);

        if (userId != null)
        {
            // Criar ClaimsIdentity e Principal
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                new System.Security.Claims.Claim("ApiKey", extractedApiKey!),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "API User")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "ApiKey");
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);

            // Manter compatibilidade com código legado se houver
            context.Items["UserId"] = userId;
        }

        await _next(context);
    }
}
