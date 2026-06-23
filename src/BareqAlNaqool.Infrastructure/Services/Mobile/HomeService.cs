using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

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
