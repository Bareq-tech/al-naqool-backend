using System.Globalization;
using System.Text.Json;
using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class AppDataHelper(AppDbContext db, ILanguageContext languageContext)
{
    public string Lang => languageContext.Language;

    public async Task<List<string>> GetAppSettingListAsync(string key, CancellationToken cancellationToken)
    {
        var setting = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            return [];
        }

        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        if (root.TryGetProperty(Lang, out var values))
        {
            return values.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
        }

        return [];
    }
}

public class HomeService(AppDbContext db, AppDataHelper helper) : IHomeService
{
    public async Task<HomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await db.HomeStats.AsNoTracking().FirstAsync(cancellationToken);
        return new HomeStatsDto(stats.TotalMembers, stats.FamilyBranches, stats.NewsUpdates, stats.UpcomingEvents);
    }

    public async Task<NewsItemDto> GetLatestNewsAsync(CancellationToken cancellationToken = default)
    {
        var item = await db.NewsItems.AsNoTracking()
            .Where(x => x.PublishStatus == PublishStatuses.Published && x.PublishedAt <= DateTime.UtcNow)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .FirstAsync(cancellationToken);
        return await MapNewsAsync(item, cancellationToken);
    }

    public async Task<IReadOnlyList<LandingSlideDto>> GetLandingSlidesAsync(CancellationToken cancellationToken = default)
    {
        var slides = await db.LandingSlides.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<LandingSlideDto>();
        foreach (var slide in slides)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.LandingSlide, slide.Id, helper.Lang, cancellationToken);
            result.Add(new LandingSlideDto(
                TranslationStore.GetString(map, "titleLine1"),
                TranslationStore.GetString(map, "titleLine2"),
                TranslationStore.GetString(map, "subtitle")));
        }

        return result;
    }

    private async Task<NewsItemDto> MapNewsAsync(NewsItem item, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.NewsItem, item.Id, helper.Lang, cancellationToken);
        return new NewsItemDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "body"),
            TimeAgoFormatter.Format(item.PublishedAt, helper.Lang),
            TranslationStore.GetString(map, "publishedDate"),
            item.CategoryColorHex,
            item.ImageUrl);
    }
}

public class NewsService(AppDbContext db, AppDataHelper helper) : INewsService
{
    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.NewsCategories, cancellationToken);

    public async Task<IReadOnlyList<NewsItemDto>> GetNewsAsync(string? category, CancellationToken cancellationToken = default)
    {
        var items = await db.NewsItems.AsNoTracking()
            .Where(x => x.PublishStatus == PublishStatuses.Published && x.PublishedAt <= DateTime.UtcNow)
            .OrderByDescending(x => x.PublishedAt)
            .ToListAsync(cancellationToken);
        var result = new List<NewsItemDto>();
        foreach (var item in items)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.NewsItem, item.Id, helper.Lang, cancellationToken);
            var itemCategory = TranslationStore.GetString(map, "category");
            if (category is not null && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
            {
                var normalized = category.Replace("s", "", StringComparison.Ordinal);
                if (!itemCategory.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            result.Add(new NewsItemDto(
                IdFormatter.ToStringId(item.Id),
                itemCategory,
                TranslationStore.GetString(map, "title"),
                TranslationStore.GetString(map, "description"),
                TranslationStore.GetString(map, "body"),
                TimeAgoFormatter.Format(item.PublishedAt, helper.Lang),
                TranslationStore.GetString(map, "publishedDate"),
                item.CategoryColorHex,
                item.ImageUrl));
        }

        return result;
    }

    public async Task<NewsItemDto?> GetNewsByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.NewsItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return null;
        }

        var map = await TranslationStore.GetMapAsync(db, EntityTypes.NewsItem, item.Id, helper.Lang, cancellationToken);
        return new NewsItemDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "body"),
            TimeAgoFormatter.Format(item.PublishedAt, helper.Lang),
            TranslationStore.GetString(map, "publishedDate"),
            item.CategoryColorHex,
            item.ImageUrl);
    }
}

public class EventsService(AppDbContext db, AppDataHelper helper) : IEventsService
{
    public async Task<IReadOnlyList<EventItemDto>> GetEventsAsync(string filter, int? userId, CancellationToken cancellationToken = default)
    {
        var items = await db.Events.AsNoTracking()
            .Where(x => x.IsPublic)
            .OrderBy(x => x.EventDate)
            .ToListAsync(cancellationToken);
        var registrations = userId.HasValue
            ? await db.EventRegistrations.AsNoTracking().Where(x => x.UserId == userId.Value).Select(x => x.EventId).ToListAsync(cancellationToken)
            : [];

        var result = new List<EventItemDto>();
        foreach (var item in items)
        {
            var dto = await MapEventAsync(item, registrations.Contains(item.Id), cancellationToken);
            result.Add(dto);
        }

        return filter switch
        {
            "Upcoming" => result.Take(3).ToList(),
            "My Events" => result.Where(x => x.IsMine).ToList(),
            _ => result
        };
    }

    public async Task<EventItemDto?> GetEventByIdAsync(string id, int? userId, CancellationToken cancellationToken = default)
    {
        var item = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return null;
        }

        var isMine = userId.HasValue && await db.EventRegistrations.AnyAsync(x => x.EventId == item.Id && x.UserId == userId.Value, cancellationToken);
        return await MapEventAsync(item, isMine, cancellationToken);
    }

    public async Task<bool> IsRegisteredAsync(string eventId, int userId, CancellationToken cancellationToken = default)
        => await db.EventRegistrations.AnyAsync(x => x.EventId == IdFormatter.ParseId(eventId) && x.UserId == userId, cancellationToken);

    public async Task RegisterForEventAsync(string eventId, int userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(eventId);
        var exists = await db.EventRegistrations.AnyAsync(x => x.EventId == parsedId && x.UserId == userId, cancellationToken);
        if (!exists)
        {
            db.EventRegistrations.Add(new EventRegistration
            {
                EventId = parsedId,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<EventItemDto> MapEventAsync(EventItem item, bool isMine, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.EventItem, item.Id, helper.Lang, cancellationToken);
        return new EventItemDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "day"),
            TranslationStore.GetString(map, "month"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "location"),
            TranslationStore.GetString(map, "time"),
            TranslationStore.GetString(map, "fullDate"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "organizer"),
            isMine);
    }
}

public class FamilyTreeService(AppDbContext db, AppDataHelper helper) : IFamilyTreeService
{
    public async Task<IReadOnlyList<FamilyBranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        var branches = await db.FamilyBranches.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<FamilyBranchDto>();
        foreach (var branch in branches)
        {
            result.Add(await MapBranchAsync(branch, cancellationToken));
        }

        return result;
    }

    public async Task<FamilyBranchDto?> GetBranchByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var branch = await db.FamilyBranches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return branch is null ? null : await MapBranchAsync(branch, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchMemberDto>> GetBranchMembersAsync(string branchId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(branchId);
        var members = await db.BranchMembers.AsNoTracking().Where(x => x.BranchId == parsedId).ToListAsync(cancellationToken);
        var result = new List<BranchMemberDto>();
        foreach (var member in members)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.BranchMember, member.Id, helper.Lang, cancellationToken);
            result.Add(new BranchMemberDto(
                IdFormatter.ToStringId(member.Id),
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "role"),
                IdFormatter.ToStringId(member.BranchId),
                member.ImageUrl));
        }

        return result;
    }

    public async Task<IReadOnlyList<TreeMemberDto>> GetFounderLineageAsync(CancellationToken cancellationToken = default)
    {
        var members = await db.TreeMembers.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<TreeMemberDto>();
        foreach (var member in members)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.TreeMember, member.Id, helper.Lang, cancellationToken);
            result.Add(new TreeMemberDto(
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "subtitle"),
                member.Generation,
                member.IsFounder,
                member.ImageUrl));
        }

        return result;
    }

    private async Task<FamilyBranchDto> MapBranchAsync(FamilyBranch branch, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, branch.Id, helper.Lang, cancellationToken);
        return new FamilyBranchDto(
            IdFormatter.ToStringId(branch.Id),
            TranslationStore.GetString(map, "name"),
            branch.MemberCount,
            TranslationStore.GetString(map, "description"),
            branch.ImageUrl);
    }
}
