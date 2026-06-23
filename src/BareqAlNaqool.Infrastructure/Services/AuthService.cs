using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace BareqAlNaqool.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(request.Username)
                   ?? await userManager.FindByEmailAsync(request.Username);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.Phone,
            FullName = request.FullName,
            DisplayRole = request.Relation,
            MemberId = $"AN-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}",
            Branch = request.Relation,
            DateOfBirth = request.DateOfBirth,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, AppRoles.Member);
        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponseDto> ContinueAsGuestAsync(CancellationToken cancellationToken = default)
    {
        var username = $"guest_{Guid.NewGuid():N}"[..20];
        var user = new ApplicationUser
        {
            UserName = username,
            Email = $"{username}@guest.local",
            FullName = "Guest",
            DisplayRole = "Guest",
            IsGuest = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, $"Guest!{Guid.NewGuid():N}");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, AppRoles.Guest);
        return await BuildResponseAsync(user);
    }

    public async Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await signInManager.SignOutAsync();
        }
    }

    public Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public async Task<AuthSessionDto?> GetSessionAsync(int? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthSessionDto(JwtTokenService.GetMode(user, roles), user.FullName, user.Email);
    }

    private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var session = new AuthSessionDto(JwtTokenService.GetMode(user, roles), user.FullName, user.Email);
        var token = await jwtTokenService.CreateTokenAsync(user);
        return new AuthResponseDto(session, token);
    }
}
