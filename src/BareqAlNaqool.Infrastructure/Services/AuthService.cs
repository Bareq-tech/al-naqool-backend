using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Persistence;
using BareqAlNaqool.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BareqAlNaqool.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwtTokenService,
    AppDbContext db) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(request.Username)
                   ?? await userManager.FindByEmailAsync(request.Username);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Registration is pending admin approval.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Registration was rejected.");
        }

        if (string.Equals(user.AccountStatus, AccountStatuses.Suspended, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Account is suspended.");
        }

        return await BuildResponseAsync(user);
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!request.AcceptTerms)
        {
            throw new InvalidOperationException("Terms and conditions must be accepted.");
        }

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
            RegistrationRelation = request.Relation,
            AccountStatus = AccountStatuses.Pending,
            TermsAcceptedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        return new RegisterResponseDto(AccountStatuses.Pending, "Registration submitted for admin approval.");
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
            AccountStatus = AccountStatuses.Active,
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

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return;
        }

        db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            UserId = user.Id,
            Email = email,
            RequestedAt = DateTime.UtcNow,
            IsResolved = false
        });
        await db.SaveChangesAsync(cancellationToken);
    }

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

    public async Task<TermsDto> GetTermsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await db.AppSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == AppSettingKeys.TermsAndConditions, cancellationToken);
        if (setting is null)
        {
            return new TermsDto(string.Empty, string.Empty);
        }

        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return new TermsDto(
            root.TryGetProperty("en", out var en) ? en.GetString() ?? string.Empty : string.Empty,
            root.TryGetProperty("ar", out var ar) ? ar.GetString() ?? string.Empty : string.Empty);
    }

    private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var session = new AuthSessionDto(JwtTokenService.GetMode(user, roles), user.FullName, user.Email);
        var token = await jwtTokenService.CreateTokenAsync(user);
        return new AuthResponseDto(session, token);
    }
}
