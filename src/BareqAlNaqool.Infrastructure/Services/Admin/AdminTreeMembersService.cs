using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminTreeMembersService(AppDbContext db) : IAdminCrudService<AdminTreeMemberDto, AdminTreeMemberCreateDto, AdminTreeMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminTreeMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.TreeMembers.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminTreeMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminTreeMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.TreeMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminTreeMemberDto> CreateAsync(AdminTreeMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new TreeMember
        {
            Generation = dto.Generation,
            IsFounder = dto.IsFounder,
            ImageUrl = dto.ImageUrl,
            SortOrder = dto.SortOrder
        };
        db.TreeMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminTreeMemberDto?> UpdateAsync(string id, AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Generation = dto.Generation;
        entity.IsFounder = dto.IsFounder;
        entity.ImageUrl = dto.ImageUrl;
        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.TreeMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminTreeMemberDto> MapAsync(TreeMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.TreeMember, item.Id, cancellationToken);
        return new AdminTreeMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.Generation,
            item.IsFounder,
            item.ImageUrl,
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminTreeMemberCreateDto(dto.NameEn, dto.NameAr, dto.SubtitleEn, dto.SubtitleAr, dto.Generation, dto.IsFounder, dto.ImageUrl, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminTreeMemberCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.TreeMember,
            id,
            new { name = dto.NameEn, subtitle = dto.SubtitleEn },
            new { name = dto.NameAr, subtitle = dto.SubtitleAr },
            cancellationToken);
}
