using Chat.Minimal.Services.Application.DTOs;

namespace Chat.Minimal.Services.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> RegisterAsync(RegisterUserDto dto);
    Task<UserResponseDto?> LoginAsync(LoginUserDto dto);
    Task<UserResponseDto?> GetByIdAsync(string id);
    Task<UserResponseDto?> UpdateAsync(string id, RegisterUserDto dto);
    Task<bool> DeleteAsync(string id);
}
