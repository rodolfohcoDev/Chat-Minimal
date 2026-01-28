using Chat.Minimal.Services.Application.DTOs;
using Chat.Minimal.Services.Application.Interfaces;

namespace Chat.Minimal.Services.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        // Register new user
        group.MapPost("/register", async (RegisterUserDto dto, IUserService userService) =>
        {
            var result = await userService.RegisterAsync(dto);
            return result != null
                ? Results.Ok(result)
                : Results.BadRequest(new { error = "Failed to register user" });
        })
        .WithName("RegisterUser");

        // Login user
        group.MapPost("/login", async (LoginUserDto dto, IUserService userService) =>
        {
            var result = await userService.LoginAsync(dto);
            return result != null
                ? Results.Ok(result)
                : Results.Unauthorized();
        })
        .WithName("LoginUser");

        // Get user by ID (protected)
        group.MapGet("/{id}", async (string id, IUserService userService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await userService.GetByIdAsync(id);
            return result != null
                ? Results.Ok(result)
                : Results.NotFound();
        })
        .WithName("GetUser");

        // Update user (protected)
        group.MapPut("/{id}", async (string id, RegisterUserDto dto, IUserService userService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await userService.UpdateAsync(id, dto);
            return result != null
                ? Results.Ok(result)
                : Results.NotFound();
        })
        .WithName("UpdateUser");

        // Delete user (protected)
        group.MapDelete("/{id}", async (string id, IUserService userService, HttpContext context) =>
        {
            var userId = context.Items["UserId"]?.ToString();
            if (userId == null)
                return Results.Unauthorized();

            var result = await userService.DeleteAsync(id);
            return result
                ? Results.NoContent()
                : Results.NotFound();
        })
        .WithName("DeleteUser");
    }
}
