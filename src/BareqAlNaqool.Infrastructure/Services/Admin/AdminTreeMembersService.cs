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
        var items = await db.TreeMembers.AsNoTracking()
            .OrderBy(x => x.Generation)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
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
        var parentId = await ParseAndValidateParentAsync(dto.ParentId, excludeId: null, cancellationToken);
        var spouseId = await ParseAndValidateSpouseAsync(dto.SpouseId, excludeId: null, cancellationToken);
        var (generation, isFounder, sortOrder) = await ResolvePlacementAsync(parentId, cancellationToken);

        var entity = new TreeMember
        {
            ParentId = parentId,
            SpouseId = spouseId,
            Generation = generation,
            IsFounder = isFounder,
            ImageUrl = dto.ImageUrl,
            SortOrder = sortOrder
        };
        db.TreeMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        if (spouseId is not null)
        {
            await LinkSpouseAsync(entity.Id, spouseId.Value, cancellationToken);
        }

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

        var parentId = await ParseAndValidateParentAsync(dto.ParentId, entity.Id, cancellationToken);
        if (parentId == entity.Id)
        {
            parentId = entity.ParentId;
        }

        var spouseId = await ParseAndValidateSpouseAsync(dto.SpouseId, entity.Id, cancellationToken);
        if (spouseId == entity.Id)
        {
            spouseId = null;
        }

        var previousSpouseId = entity.SpouseId;
        var parentChanged = parentId != entity.ParentId;

        if (parentChanged)
        {
            var (generation, isFounder, sortOrder) = await ResolvePlacementAsync(parentId, cancellationToken, entity.Id);
            entity.Generation = generation;
            entity.IsFounder = isFounder;
            entity.SortOrder = sortOrder;
        }
        else
        {
            entity.IsFounder = parentId is null && entity.IsFounder;
            if (parentId is null && !entity.IsFounder)
            {
                var hasOtherFounder = await db.TreeMembers.AnyAsync(
                    x => x.IsFounder && x.Id != entity.Id,
                    cancellationToken);
                entity.IsFounder = !hasOtherFounder;
            }
        }

        entity.ParentId = parentId;
        entity.SpouseId = spouseId;
        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);

        if (previousSpouseId != spouseId)
        {
            if (previousSpouseId is not null)
            {
                await ClearSpouseLinkAsync(previousSpouseId.Value, entity.Id, cancellationToken);
            }

            if (spouseId is not null)
            {
                await LinkSpouseAsync(entity.Id, spouseId.Value, cancellationToken);
            }
        }

        if (parentChanged)
        {
            await RecomputeDescendantGenerationsAsync(entity.Id, entity.Generation, cancellationToken);
        }

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

        if (entity.SpouseId is not null)
        {
            await ClearSpouseLinkAsync(entity.SpouseId.Value, entity.Id, cancellationToken);
        }

        var children = await db.TreeMembers.Where(x => x.ParentId == entity.Id).ToListAsync(cancellationToken);
        foreach (var child in children)
        {
            child.ParentId = entity.ParentId;
            child.Generation = entity.ParentId is null
                ? 0
                : entity.Generation;
            if (entity.ParentId is null)
            {
                var hasOtherFounder = await db.TreeMembers.AnyAsync(
                    x => x.IsFounder && x.Id != entity.Id && x.Id != child.Id,
                    cancellationToken);
                child.IsFounder = !hasOtherFounder && children.IndexOf(child) == 0;
            }
            else
            {
                child.IsFounder = false;
            }
        }

        db.TreeMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var child in children)
        {
            await RecomputeDescendantGenerationsAsync(child.Id, child.Generation, cancellationToken);
        }

        return true;
    }

    private async Task<int?> ParseAndValidateParentAsync(
        string? rawId,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var parentId = ParseOptionalId(rawId);
        if (parentId is null)
        {
            return null;
        }

        if (excludeId is not null && parentId == excludeId)
        {
            throw new ArgumentException("A member cannot be their own parent.");
        }

        var exists = await db.TreeMembers.AnyAsync(x => x.Id == parentId.Value, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException($"Parent member '{rawId}' was not found.");
        }

        if (excludeId is not null && await WouldCreateCycleAsync(excludeId.Value, parentId.Value, cancellationToken))
        {
            throw new ArgumentException("Cannot set parent: that would create a cycle in the family tree.");
        }

        return parentId;
    }

    private async Task<bool> WouldCreateCycleAsync(int memberId, int proposedParentId, CancellationToken cancellationToken)
    {
        var currentId = (int?)proposedParentId;
        var guard = 0;
        while (currentId is not null && guard++ < 10_000)
        {
            if (currentId == memberId)
            {
                return true;
            }

            currentId = await db.TreeMembers.AsNoTracking()
                .Where(x => x.Id == currentId.Value)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }

    private async Task<int?> ParseAndValidateSpouseAsync(
        string? rawId,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var spouseId = ParseOptionalId(rawId);
        if (spouseId is null)
        {
            return null;
        }

        if (excludeId is not null && spouseId == excludeId)
        {
            throw new ArgumentException("A member cannot be their own spouse.");
        }

        var exists = await db.TreeMembers.AnyAsync(x => x.Id == spouseId.Value, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException($"Spouse member '{rawId}' was not found.");
        }

        return spouseId;
    }

    private async Task<(int Generation, bool IsFounder, int SortOrder)> ResolvePlacementAsync(
        int? parentId,
        CancellationToken cancellationToken,
        int? excludeId = null)
    {
        if (parentId is null)
        {
            var hasOtherFounder = await db.TreeMembers.AnyAsync(
                x => x.IsFounder && (excludeId == null || x.Id != excludeId),
                cancellationToken);
            var rootSortOrder = await NextSortOrderAsync(null, excludeId, cancellationToken);
            return (0, !hasOtherFounder, rootSortOrder);
        }

        var parent = await db.TreeMembers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == parentId.Value, cancellationToken)
            ?? throw new ArgumentException($"Parent member '{parentId}' was not found.");

        var sortOrder = await NextSortOrderAsync(parentId, excludeId, cancellationToken);
        return (parent.Generation + 1, false, sortOrder);
    }

    private async Task<int> NextSortOrderAsync(int? parentId, int? excludeId, CancellationToken cancellationToken)
    {
        var query = db.TreeMembers.AsNoTracking().Where(x => x.ParentId == parentId);
        if (excludeId is not null)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        var maxSort = await query.Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken);
        return (maxSort ?? -1) + 1;
    }

    private async Task LinkSpouseAsync(int memberId, int spouseId, CancellationToken cancellationToken)
    {
        var spouse = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == spouseId, cancellationToken);
        if (spouse is null)
        {
            return;
        }

        // Clear reverse link from the spouse's previous partner.
        if (spouse.SpouseId is not null && spouse.SpouseId != memberId)
        {
            var previousPartner = await db.TreeMembers.FirstOrDefaultAsync(
                x => x.Id == spouse.SpouseId.Value,
                cancellationToken);
            if (previousPartner is not null && previousPartner.SpouseId == spouseId)
            {
                previousPartner.SpouseId = null;
            }
        }

        spouse.SpouseId = memberId;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ClearSpouseLinkAsync(int spouseId, int memberId, CancellationToken cancellationToken)
    {
        var spouse = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == spouseId, cancellationToken);
        if (spouse is null || spouse.SpouseId != memberId)
        {
            return;
        }

        spouse.SpouseId = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RecomputeDescendantGenerationsAsync(
        int memberId,
        int memberGeneration,
        CancellationToken cancellationToken)
    {
        var children = await db.TreeMembers.Where(x => x.ParentId == memberId).ToListAsync(cancellationToken);
        foreach (var child in children)
        {
            child.Generation = memberGeneration + 1;
            child.IsFounder = false;
            await RecomputeDescendantGenerationsAsync(child.Id, child.Generation, cancellationToken);
        }

        if (children.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static int? ParseOptionalId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return IdFormatter.ParseId(id);
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
            item.ParentId is null ? null : IdFormatter.ToStringId(item.ParentId.Value),
            item.SpouseId is null ? null : IdFormatter.ToStringId(item.SpouseId.Value),
            item.Generation,
            item.IsFounder,
            item.ImageUrl,
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminTreeMemberCreateDto(dto.NameEn, dto.NameAr, dto.SubtitleEn, dto.SubtitleAr, dto.ParentId, dto.SpouseId, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminTreeMemberCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.TreeMember,
            id,
            new { name = dto.NameEn, subtitle = dto.SubtitleEn },
            new { name = dto.NameAr, subtitle = dto.SubtitleAr },
            cancellationToken);
}
