using System.Globalization;
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

public class AdminUsersService(UserManager<ApplicationUser> userManager) : IAdminCrudService<AdminUserDto, AdminUserCreateDto, AdminUserUpdateDto>
{
    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users.ToListAsync(cancellationToken);
        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            result.Add(await MapAsync(user));
        }

        return result;
    }

    public async Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is null ? null : await MapAsync(user);
    }

    public async Task<AdminUserDto> CreateAsync(AdminUserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.Phone,
            FullName = dto.FullName,
            DisplayRole = dto.Role,
            MemberId = dto.MemberId,
            Branch = dto.Branch,
            DateOfBirth = dto.DateOfBirth,
            MaritalStatus = dto.MaritalStatus,
            ChildrenCount = dto.ChildrenCount,
            AccountStatus = dto.AccountStatus,
            IsGuest = dto.Role == AppRoles.Guest,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await AssignRoleAsync(user, dto.Role);
        return await MapAsync(user);
    }

    public async Task<AdminUserDto?> UpdateAsync(string id, AdminUserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        user.Email = dto.Email;
        user.PhoneNumber = dto.Phone;
        user.FullName = dto.FullName;
        user.DisplayRole = dto.Role;
        user.MemberId = dto.MemberId;
        user.Branch = dto.Branch;
        user.DateOfBirth = dto.DateOfBirth;
        user.MaritalStatus = dto.MaritalStatus;
        user.ChildrenCount = dto.ChildrenCount;
        user.AccountStatus = dto.AccountStatus;
        user.IsGuest = dto.Role == AppRoles.Guest;
        await userManager.UpdateAsync(user);
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.Password);
        }

        await AssignRoleAsync(user, dto.Role);
        return await MapAsync(user);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private async Task AssignRoleAsync(ApplicationUser user, string role)
    {
        if (!AppRoles.All.Contains(role))
        {
            throw new InvalidOperationException($"Invalid role: {role}");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, role);
    }

    private async Task<AdminUserDto> MapAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AdminUserDto(
            user.Id.ToString(CultureInfo.InvariantCulture),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? AppRoles.Member,
            user.MemberId,
            user.Branch,
            user.PhoneNumber ?? string.Empty,
            user.DateOfBirth,
            user.MaritalStatus,
            user.ChildrenCount,
            user.AccountStatus,
            user.RegistrationRelation,
            user.IsGuest,
            user.TermsAcceptedAt);
    }
}
