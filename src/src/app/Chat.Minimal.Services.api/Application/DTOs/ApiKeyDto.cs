namespace Chat.Minimal.Services.Application.DTOs;

// ApiKey DTOs
public record CreateApiKeyDto(
    string Name,
    DateTime? ExpiresAt = null
);

public record ApiKeyResponseDto(
    Guid Id,
    string Key,
    string Name,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsActive
);
