using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminGalleryPhotosService(AppDbContext db) : IAdminCrudService<AdminGalleryPhotoDto, AdminGalleryPhotoCreateDto, AdminGalleryPhotoUpdateDto>
{
    public async Task<IReadOnlyList<AdminGalleryPhotoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.GalleryPhotos.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AdminGalleryPhotoDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminGalleryPhotoDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.GalleryPhotos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminGalleryPhotoDto> CreateAsync(AdminGalleryPhotoCreateDto dto, CancellationToken cancellationToken = default)
    {
        var albumId = IdFormatter.ParseId(dto.AlbumId);
        var album = await db.Albums.FirstAsync(x => x.Id == albumId, cancellationToken);
        var entity = new GalleryPhoto
        {
            AlbumId = albumId,
            ImageUrl = dto.ImageUrl
        };
        db.GalleryPhotos.Add(entity);
        album.PhotoCount++;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminGalleryPhotoDto?> UpdateAsync(string id, AdminGalleryPhotoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.GalleryPhotos.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.GalleryPhotos.Include(x => x.Album).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Album.PhotoCount = Math.Max(0, entity.Album.PhotoCount - 1);
        db.GalleryPhotos.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminGalleryPhotoDto> MapAsync(GalleryPhoto item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.GalleryPhoto, item.Id, cancellationToken);
        return new AdminGalleryPhotoDto(
            IdFormatter.ToStringId(item.Id),
            IdFormatter.ToStringId(item.AlbumId),
            AdminTranslationHelper.Get(en, "caption"),
            AdminTranslationHelper.Get(ar, "caption"),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminGalleryPhotoUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminGalleryPhotoCreateDto(string.Empty, dto.CaptionEn, dto.CaptionAr, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminGalleryPhotoCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.GalleryPhoto,
            id,
            new { caption = dto.CaptionEn },
            new { caption = dto.CaptionAr },
            cancellationToken);
}
