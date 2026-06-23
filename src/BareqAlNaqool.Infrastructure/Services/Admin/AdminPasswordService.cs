using System.Text.Json;
using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminPasswordService(AppDbContext db, UserManager<ApplicationUser> userManager) : IAdminPasswordService
{
    public async Task<IReadOnlyList<AdminPasswordResetRequestDto>> GetPendingResetsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await db.PasswordResetRequests.AsNoTracking()
            .Where(x => !x.IsResolved)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
        return requests.Select(Map).ToList();
    }

    public async Task<bool> ResetPasswordAsync(string userId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        return await SetPasswordAsync(user, dto.NewPassword);
    }

    public async Task<bool> ResolveResetRequestAsync(string requestId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var request = await db.PasswordResetRequests.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(requestId), cancellationToken);
        if (request is null || request.IsResolved)
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return false;
        }

        if (!await SetPasswordAsync(user, dto.NewPassword))
        {
            return false;
        }

        request.IsResolved = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> SetPasswordAsync(ApplicationUser user, string newPassword)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    private static AdminPasswordResetRequestDto Map(PasswordResetRequest request) =>
        new(
            IdFormatter.ToStringId(request.Id),
            IdFormatter.ToStringId(request.UserId),
            request.Email,
            request.RequestedAt,
            request.IsResolved);
}
