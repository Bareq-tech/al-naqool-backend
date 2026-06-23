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

public class AdminAlbumsService(AppDbContext db) : IAdminCrudService<AdminAlbumDto, AdminAlbumCreateDto, AdminAlbumUpdateDto>
{
    public async Task<IReadOnlyList<AdminAlbumDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Albums.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminAlbumDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminAlbumDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Albums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminAlbumDto> CreateAsync(AdminAlbumCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Album
        {
            PhotoCount = dto.PhotoCount,
            ImageUrl = dto.ImageUrl,
            IsFeatured = dto.IsFeatured,
            GalleryTypeKey = dto.GalleryTypeKey
        };
        db.Albums.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminAlbumDto?> UpdateAsync(string id, AdminAlbumUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Albums.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.PhotoCount = dto.PhotoCount;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsFeatured = dto.IsFeatured;
        entity.GalleryTypeKey = dto.GalleryTypeKey;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Albums.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Albums.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminAlbumDto> MapAsync(Album item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.Album, item.Id, cancellationToken);
        return new AdminAlbumDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            item.PhotoCount,
            item.ImageUrl,
            item.IsFeatured,
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            item.GalleryTypeKey);
    }

    private Task SaveTranslationsAsync(int id, AdminAlbumUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminAlbumCreateDto(
            dto.TitleEn, dto.TitleAr, dto.PhotoCount, dto.ImageUrl, dto.IsFeatured, dto.DescriptionEn, dto.DescriptionAr, dto.GalleryTypeKey),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminAlbumCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Album,
            id,
            new { title = dto.TitleEn, description = dto.DescriptionEn },
            new { title = dto.TitleAr, description = dto.DescriptionAr },
            cancellationToken);
}
