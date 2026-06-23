using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

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
