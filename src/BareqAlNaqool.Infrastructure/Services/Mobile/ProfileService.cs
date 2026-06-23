using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class ProfileService(AppDbContext db) : IProfileService
{
    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId, cancellationToken);
        return MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstAsync(x => x.Id == userId, cancellationToken);
        user.FullName = request.Name;
        user.DisplayRole = request.Role;
        user.MemberId = request.MemberId;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.Branch = request.Branch;
        user.DateOfBirth = request.DateOfBirth;
        user.MaritalStatus = request.MaritalStatus;
        user.ChildrenCount = request.ChildrenCount;
        await db.SaveChangesAsync(cancellationToken);
        return MapProfile(user);
    }

    private static UserProfileDto MapProfile(ApplicationUser user) => new(
        user.FullName,
        user.DisplayRole,
        user.MemberId,
        user.Email ?? string.Empty,
        user.PhoneNumber ?? string.Empty,
        user.Branch,
        user.DateOfBirth,
        user.MaritalStatus,
        user.ChildrenCount);
}
