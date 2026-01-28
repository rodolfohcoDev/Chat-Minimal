using Chat.Minimal.Services.Application.DTOs;
using Chat.Minimal.Services.Application.Interfaces;
using Chat.Minimal.Services.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chat.Minimal.Services.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<UserResponseDto?> RegisterAsync(RegisterUserDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            UserName = dto.UserName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return null;

        return new UserResponseDto(
            user.Id,
            user.Email!,
            user.UserName!,
            user.CreatedAt,
            user.IsActive
        );
    }

    public async Task<UserResponseDto?> LoginAsync(LoginUserDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return null;

        return new UserResponseDto(
            user.Id,
            user.Email!,
            user.UserName!,
            user.CreatedAt,
            user.IsActive
        );
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;

        return new UserResponseDto(
            user.Id,
            user.Email!,
            user.UserName!,
            user.CreatedAt,
            user.IsActive
        );
    }

    public async Task<UserResponseDto?> UpdateAsync(string id, RegisterUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;

        user.Email = dto.Email;
        user.UserName = dto.UserName;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return null;

        return new UserResponseDto(
            user.Id,
            user.Email!,
            user.UserName!,
            user.CreatedAt,
            user.IsActive
        );
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
