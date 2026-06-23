using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        return new DashboardStatsDto(
            await db.Users.CountAsync(cancellationToken),
            await db.NewsItems.CountAsync(cancellationToken),
            await db.Events.CountAsync(cancellationToken),
            await db.FamilyBranches.CountAsync(cancellationToken),
            await db.Albums.CountAsync(cancellationToken),
            await db.Documents.CountAsync(cancellationToken),
            await db.Notifications.CountAsync(cancellationToken),
            await db.DirectoryMembers.CountAsync(cancellationToken),
            await db.ContactSubmissions.CountAsync(x => !x.IsRead, cancellationToken));
    }
}
