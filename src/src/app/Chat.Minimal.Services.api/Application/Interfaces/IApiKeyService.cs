using Chat.Minimal.Services.Application.DTOs;

namespace Chat.Minimal.Services.Application.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyResponseDto?> GenerateAsync(string userId, CreateApiKeyDto dto);
    Task<IEnumerable<ApiKeyResponseDto>> GetUserApiKeysAsync(string userId);
    Task<bool> ValidateAsync(string key);
    Task<bool> RevokeAsync(Guid id, string userId);
    Task<string?> GetUserIdByApiKeyAsync(string key);
}
