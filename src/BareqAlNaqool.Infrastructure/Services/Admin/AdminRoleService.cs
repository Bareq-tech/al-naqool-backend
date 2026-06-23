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

public class AdminRoleService(UserManager<ApplicationUser> userManager) : IAdminRoleService
{
    public Task<IReadOnlyList<AdminRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AdminRoleDto>>(AppRoles.All.Select(x => new AdminRoleDto(x)).ToList());

    public async Task<bool> AssignRoleAsync(string userId, AdminAssignRoleDto dto, CancellationToken cancellationToken = default)
    {
        if (!AppRoles.All.Contains(dto.Role))
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        var result = await userManager.AddToRoleAsync(user, dto.Role);
        if (result.Succeeded)
        {
            user.DisplayRole = dto.Role;
            await userManager.UpdateAsync(user);
        }

        return result.Succeeded;
    }
}
