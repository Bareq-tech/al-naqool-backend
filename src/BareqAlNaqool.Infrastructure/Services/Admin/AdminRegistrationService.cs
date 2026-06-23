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

public class AdminRegistrationService(UserManager<ApplicationUser> userManager) : IAdminRegistrationService
{
    public async Task<IReadOnlyList<AdminRegistrationDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Where(x => x.AccountStatus == AccountStatuses.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<bool> ApproveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.AccountStatus != AccountStatuses.Pending)
        {
            return false;
        }

        user.AccountStatus = AccountStatuses.Active;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        await userManager.AddToRoleAsync(user, AppRoles.Member);
        return true;
    }

    public async Task<bool> RejectAsync(string userId, AdminRegistrationDecisionDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.AccountStatus != AccountStatuses.Pending)
        {
            return false;
        }

        user.AccountStatus = AccountStatuses.Rejected;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private static AdminRegistrationDto Map(ApplicationUser user) =>
        new(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.PhoneNumber ?? string.Empty,
            user.DateOfBirth,
            user.RegistrationRelation ?? user.DisplayRole,
            user.CreatedAt);
}
