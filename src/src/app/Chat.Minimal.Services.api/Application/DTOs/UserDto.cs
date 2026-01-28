namespace Chat.Minimal.Services.Application.DTOs;

// User DTOs
public record RegisterUserDto(
    string Email,
    string Password,
    string UserName
);

public record LoginUserDto(
    string Email,
    string Password
);

public record UserResponseDto(
    string Id,
    string Email,
    string UserName,
    DateTime CreatedAt,
    bool IsActive
);
