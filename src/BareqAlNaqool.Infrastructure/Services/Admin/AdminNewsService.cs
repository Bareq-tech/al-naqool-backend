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

public class AdminNewsService(AppDbContext db) : IAdminCrudService<AdminNewsDto, AdminNewsCreateDto, AdminNewsUpdateDto>
{
    public async Task<IReadOnlyList<AdminNewsDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.NewsItems.AsNoTracking().OrderByDescending(x => x.PublishedAt).ToListAsync(cancellationToken);
        var result = new List<AdminNewsDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminNewsDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.NewsItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminNewsDto> CreateAsync(AdminNewsCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new NewsItem
        {
            CategoryKey = dto.CategoryEn,
            CategoryColorHex = dto.CategoryColorHex,
            ImageUrl = dto.ImageUrl,
            IsFeatured = dto.IsFeatured,
            PublishStatus = dto.PublishStatus,
            PublishedAt = dto.PublishedAt ?? DateTime.UtcNow
        };
        db.NewsItems.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminNewsDto?> UpdateAsync(string id, AdminNewsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.NewsItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CategoryKey = dto.CategoryEn;
        entity.CategoryColorHex = dto.CategoryColorHex;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsFeatured = dto.IsFeatured;
        entity.PublishStatus = dto.PublishStatus;
        if (dto.PublishedAt.HasValue)
        {
            entity.PublishedAt = dto.PublishedAt.Value;
        }

        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.NewsItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.NewsItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminNewsDto> MapAsync(NewsItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.NewsItem, item.Id, cancellationToken);
        return new AdminNewsDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "category"),
            AdminTranslationHelper.Get(ar, "category"),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            AdminTranslationHelper.Get(en, "body"),
            AdminTranslationHelper.Get(ar, "body"),
            AdminTranslationHelper.Get(en, "publishedDate"),
            AdminTranslationHelper.Get(ar, "publishedDate"),
            item.CategoryColorHex,
            item.ImageUrl,
            item.IsFeatured,
            item.PublishStatus,
            item.PublishedAt);
    }

    private Task SaveTranslationsAsync(int id, AdminNewsUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminNewsCreateDto(
            dto.CategoryEn, dto.CategoryAr, dto.TitleEn, dto.TitleAr, dto.DescriptionEn, dto.DescriptionAr,
            dto.BodyEn, dto.BodyAr, dto.PublishedDateEn, dto.PublishedDateAr, dto.CategoryColorHex, dto.ImageUrl,
            dto.IsFeatured, dto.PublishStatus, dto.PublishedAt), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminNewsCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.NewsItem,
            id,
            new { category = dto.CategoryEn, title = dto.TitleEn, description = dto.DescriptionEn, body = dto.BodyEn, publishedDate = dto.PublishedDateEn },
            new { category = dto.CategoryAr, title = dto.TitleAr, description = dto.DescriptionAr, body = dto.BodyAr, publishedDate = dto.PublishedDateAr },
            cancellationToken);
}
