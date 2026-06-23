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

public class AdminCouncilItemsService(AppDbContext db) : IAdminCrudService<AdminCouncilItemDto, AdminCouncilItemCreateDto, AdminCouncilItemUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilListItems.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminCouncilItemDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilListItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilItemDto> CreateAsync(AdminCouncilItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilListItem
        {
            ModuleKey = dto.ModuleId,
            StatusKey = dto.Status
        };
        db.CouncilListItems.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilItemDto?> UpdateAsync(string id, AdminCouncilItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilListItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleKey = dto.ModuleId;
        entity.StatusKey = dto.Status;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilListItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilListItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilItemDto> MapAsync(CouncilListItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilListItem, item.Id, cancellationToken);
        return new AdminCouncilItemDto(
            IdFormatter.ToStringId(item.Id),
            item.ModuleKey,
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            AdminTranslationHelper.Get(en, "meta"),
            AdminTranslationHelper.Get(ar, "meta"),
            item.StatusKey ?? AdminTranslationHelper.Get(en, "status"));
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilItemUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilItemCreateDto(
            dto.ModuleId, dto.TitleEn, dto.TitleAr, dto.SubtitleEn, dto.SubtitleAr, dto.MetaEn, dto.MetaAr, dto.Status),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilItemCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilListItem,
            id,
            new { title = dto.TitleEn, subtitle = dto.SubtitleEn, meta = dto.MetaEn, status = dto.Status ?? string.Empty },
            new { title = dto.TitleAr, subtitle = dto.SubtitleAr, meta = dto.MetaAr, status = dto.Status ?? string.Empty },
            cancellationToken);
}
