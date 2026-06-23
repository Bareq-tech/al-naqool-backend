using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminCouncilModulesService(AppDbContext db) : IAdminCrudService<AdminCouncilModuleDto, AdminCouncilModuleCreateDto, AdminCouncilModuleUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilModuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilModules.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminCouncilModuleDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilModuleDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilModules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilModuleDto> CreateAsync(AdminCouncilModuleCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilModule
        {
            ModuleKey = dto.ModuleKey,
            IconName = dto.IconName,
            SortOrder = dto.SortOrder
        };
        db.CouncilModules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilModuleDto?> UpdateAsync(string id, AdminCouncilModuleUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilModules.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleKey = dto.ModuleKey;
        entity.IconName = dto.IconName;
        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilModules.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilModules.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilModuleDto> MapAsync(CouncilModule item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilModule, item.Id, cancellationToken);
        return new AdminCouncilModuleDto(
            IdFormatter.ToStringId(item.Id),
            item.ModuleKey,
            item.IconName,
            AdminTranslationHelper.Get(en, "label"),
            AdminTranslationHelper.Get(ar, "label"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilModuleUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilModuleCreateDto(dto.ModuleKey, dto.IconName, dto.LabelEn, dto.LabelAr, dto.SubtitleEn, dto.SubtitleAr, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilModuleCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilModule,
            id,
            new { id = dto.ModuleKey, label = dto.LabelEn, subtitle = dto.SubtitleEn },
            new { id = dto.ModuleKey, label = dto.LabelAr, subtitle = dto.SubtitleAr },
            cancellationToken);
}
