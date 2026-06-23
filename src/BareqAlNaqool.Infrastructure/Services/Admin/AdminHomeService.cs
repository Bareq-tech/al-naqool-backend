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

public class AdminHomeService(AppDbContext db) : IAdminHomeService
{
    private const int SingletonId = 1;

    public async Task<AdminHomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await GetOrCreateAsync(cancellationToken);
        return Map(stats);
    }

    public async Task<AdminHomeStatsDto> UpdateHomeStatsAsync(AdminHomeStatsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var stats = await GetOrCreateAsync(cancellationToken);
        stats.TotalMembers = dto.TotalMembers;
        stats.FamilyBranches = dto.FamilyBranches;
        stats.NewsUpdates = dto.NewsUpdates;
        stats.UpcomingEvents = dto.UpcomingEvents;
        await db.SaveChangesAsync(cancellationToken);
        return Map(stats);
    }

    private async Task<HomeStat> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var stats = await db.HomeStats.FirstOrDefaultAsync(x => x.Id == SingletonId, cancellationToken);
        if (stats is not null)
        {
            return stats;
        }

        stats = new HomeStat { Id = SingletonId };
        db.HomeStats.Add(stats);
        await db.SaveChangesAsync(cancellationToken);
        return stats;
    }

    private static AdminHomeStatsDto Map(HomeStat stats) =>
        new(stats.TotalMembers, stats.FamilyBranches, stats.NewsUpdates, stats.UpcomingEvents);
}
