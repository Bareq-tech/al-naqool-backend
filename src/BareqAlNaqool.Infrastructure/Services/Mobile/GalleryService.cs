using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class GalleryService(AppDbContext db, AppDataHelper helper) : IGalleryService
{
    public async Task<IReadOnlyList<string>> GetGalleryTypesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.GalleryTypes, cancellationToken);

    public async Task<IReadOnlyList<AlbumItemDto>> GetAlbumsAsync(string? type, CancellationToken cancellationToken = default)
    {
        var albums = await db.Albums.AsNoTracking().OrderByDescending(x => x.IsFeatured).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AlbumItemDto>();
        foreach (var album in albums)
        {
            if (type is not null && !string.Equals(type, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(album.GalleryTypeKey, type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(await MapAlbumAsync(album, cancellationToken));
        }

        return result;
    }

    public async Task<AlbumItemDto?> GetAlbumByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var album = await db.Albums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return album is null ? null : await MapAlbumAsync(album, cancellationToken);
    }

    public async Task<IReadOnlyList<GalleryPhotoDto>> GetAlbumPhotosAsync(string albumId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(albumId);
        var photos = await db.GalleryPhotos.AsNoTracking().Where(x => x.AlbumId == parsedId).ToListAsync(cancellationToken);
        var result = new List<GalleryPhotoDto>();
        foreach (var photo in photos)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.GalleryPhoto, photo.Id, helper.Lang, cancellationToken);
            result.Add(new GalleryPhotoDto(
                IdFormatter.ToStringId(photo.Id),
                IdFormatter.ToStringId(photo.AlbumId),
                TranslationStore.GetString(map, "caption"),
                photo.ImageUrl));
        }

        return result;
    }

    private async Task<AlbumItemDto> MapAlbumAsync(Album album, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Album, album.Id, helper.Lang, cancellationToken);
        return new AlbumItemDto(
            IdFormatter.ToStringId(album.Id),
            TranslationStore.GetString(map, "title"),
            album.PhotoCount,
            album.ImageUrl,
            album.IsFeatured,
            TranslationStore.GetString(map, "description"));
    }
}
