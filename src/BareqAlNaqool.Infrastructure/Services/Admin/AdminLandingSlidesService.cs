using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminLandingSlidesService(AppDbContext db) : IAdminCrudService<AdminLandingSlideDto, AdminLandingSlideCreateDto, AdminLandingSlideUpdateDto>
{
    public async Task<IReadOnlyList<AdminLandingSlideDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.LandingSlides.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminLandingSlideDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminLandingSlideDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.LandingSlides.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminLandingSlideDto> CreateAsync(AdminLandingSlideCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new LandingSlide { SortOrder = dto.SortOrder };
        db.LandingSlides.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminLandingSlideDto?> UpdateAsync(string id, AdminLandingSlideUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.LandingSlides.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.LandingSlides.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.LandingSlides.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminLandingSlideDto> MapAsync(LandingSlide item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.LandingSlide, item.Id, cancellationToken);
        return new AdminLandingSlideDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "titleLine1"),
            AdminTranslationHelper.Get(ar, "titleLine1"),
            AdminTranslationHelper.Get(en, "titleLine2"),
            AdminTranslationHelper.Get(ar, "titleLine2"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminLandingSlideUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminLandingSlideCreateDto(dto.TitleLine1En, dto.TitleLine1Ar, dto.TitleLine2En, dto.TitleLine2Ar, dto.SubtitleEn, dto.SubtitleAr, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminLandingSlideCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.LandingSlide,
            id,
            new { titleLine1 = dto.TitleLine1En, titleLine2 = dto.TitleLine2En, subtitle = dto.SubtitleEn },
            new { titleLine1 = dto.TitleLine1Ar, titleLine2 = dto.TitleLine2Ar, subtitle = dto.SubtitleAr },
            cancellationToken);
}
