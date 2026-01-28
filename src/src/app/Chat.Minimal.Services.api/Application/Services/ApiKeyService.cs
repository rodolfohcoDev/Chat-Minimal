using System.Security.Cryptography;
using Chat.Minimal.Services.Application.DTOs;
using Chat.Minimal.Services.Application.Interfaces;
using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Application.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IRepository<ApiKey> _apiKeyRepository;

    public ApiKeyService(IRepository<ApiKey> apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<ApiKeyResponseDto?> GenerateAsync(string userId, CreateApiKeyDto dto)
    {
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Key = GenerateSecureKey(),
            Name = dto.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true
        };

        var created = await _apiKeyRepository.AddAsync(apiKey);

        return new ApiKeyResponseDto(
            created.Id,
            created.Key,
            created.Name,
            created.CreatedAt,
            created.ExpiresAt,
            created.IsActive
        );
    }

    public async Task<IEnumerable<ApiKeyResponseDto>> GetUserApiKeysAsync(string userId)
    {
        var apiKeys = await _apiKeyRepository.FindAsync(k => k.UserId == userId);

        return apiKeys.Select(k => new ApiKeyResponseDto(
            k.Id,
            k.Key,
            k.Name,
            k.CreatedAt,
            k.ExpiresAt,
            k.IsActive
        ));
    }

    public async Task<bool> ValidateAsync(string key)
    {
        var apiKey = (await _apiKeyRepository.FindAsync(k => k.Key == key)).FirstOrDefault();

        if (apiKey == null || !apiKey.IsActive)
            return false;

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task<bool> RevokeAsync(Guid id, string userId)
    {
        var apiKey = (await _apiKeyRepository.FindAsync(k => k.Id == id && k.UserId == userId)).FirstOrDefault();

        if (apiKey == null)
            return false;

        apiKey.IsActive = false;
        await _apiKeyRepository.UpdateAsync(apiKey);

        return true;
    }

    public async Task<string?> GetUserIdByApiKeyAsync(string key)
    {
        var apiKey = (await _apiKeyRepository.FindAsync(k => k.Key == key && k.IsActive)).FirstOrDefault();

        if (apiKey == null)
            return null;

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            return null;

        return apiKey.UserId;
    }

    private static string GenerateSecureKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
