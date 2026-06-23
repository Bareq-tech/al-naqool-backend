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

public class AdminDocumentsService(AppDbContext db) : IAdminCrudService<AdminDocumentDto, AdminDocumentCreateDto, AdminDocumentUpdateDto>
{
    public async Task<IReadOnlyList<AdminDocumentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Documents.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminDocumentDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminDocumentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminDocumentDto> CreateAsync(AdminDocumentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Document
        {
            CategoryKey = dto.Category,
            DocumentDate = ParseDocumentDate(dto.DateEn),
            FileSize = dto.FileSize,
            FileUrl = dto.FileUrl,
            AccessLevel = dto.AccessLevel
        };
        db.Documents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminDocumentDto?> UpdateAsync(string id, AdminDocumentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CategoryKey = dto.Category;
        entity.DocumentDate = ParseDocumentDate(dto.DateEn);
        entity.FileSize = dto.FileSize;
        entity.FileUrl = dto.FileUrl;
        entity.AccessLevel = dto.AccessLevel;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Documents.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DateTime ParseDocumentDate(string dateEn)
    {
        if (DateTime.TryParse(dateEn, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private async Task<AdminDocumentDto> MapAsync(Document item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.Document, item.Id, cancellationToken);
        return new AdminDocumentDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            item.FileSize,
            AdminTranslationHelper.Get(en, "date"),
            AdminTranslationHelper.Get(ar, "date"),
            item.CategoryKey,
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            item.FileUrl,
            item.AccessLevel);
    }

    private Task SaveTranslationsAsync(int id, AdminDocumentUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminDocumentCreateDto(
            dto.TitleEn, dto.TitleAr, dto.FileSize, dto.DateEn, dto.DateAr, dto.Category, dto.DescriptionEn, dto.DescriptionAr, dto.FileUrl, dto.AccessLevel),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminDocumentCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Document,
            id,
            new { title = dto.TitleEn, date = dto.DateEn, description = dto.DescriptionEn },
            new { title = dto.TitleAr, date = dto.DateAr, description = dto.DescriptionAr },
            cancellationToken);
}
