using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminBranchMembersService(AppDbContext db) : IAdminCrudService<AdminBranchMemberDto, AdminBranchMemberCreateDto, AdminBranchMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminBranchMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.BranchMembers.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AdminBranchMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminBranchMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.BranchMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminBranchMemberDto> CreateAsync(AdminBranchMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var branchId = IdFormatter.ParseId(dto.BranchId);
        await EnsureBranchExistsAsync(branchId, cancellationToken);

        var entity = new BranchMember
        {
            BranchId = branchId,
            ImageUrl = dto.ImageUrl
        };
        db.BranchMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        await RefreshBranchMemberCountAsync(branchId, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminBranchMemberDto?> UpdateAsync(string id, AdminBranchMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.BranchMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var previousBranchId = entity.BranchId;
        var branchId = IdFormatter.ParseId(dto.BranchId);
        await EnsureBranchExistsAsync(branchId, cancellationToken);

        entity.BranchId = branchId;
        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);

        await RefreshBranchMemberCountAsync(branchId, cancellationToken);
        if (previousBranchId != branchId)
        {
            await RefreshBranchMemberCountAsync(previousBranchId, cancellationToken);
        }

        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.BranchMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var branchId = entity.BranchId;
        db.BranchMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await RefreshBranchMemberCountAsync(branchId, cancellationToken);
        return true;
    }

    private async Task EnsureBranchExistsAsync(int branchId, CancellationToken cancellationToken)
    {
        var exists = await db.FamilyBranches.AnyAsync(x => x.Id == branchId, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException($"Branch '{IdFormatter.ToStringId(branchId)}' was not found.");
        }
    }

    private async Task RefreshBranchMemberCountAsync(int branchId, CancellationToken cancellationToken)
    {
        var branch = await db.FamilyBranches.FirstOrDefaultAsync(x => x.Id == branchId, cancellationToken);
        if (branch is null)
        {
            return;
        }

        branch.MemberCount = await db.BranchMembers.CountAsync(x => x.BranchId == branchId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AdminBranchMemberDto> MapAsync(BranchMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.BranchMember, item.Id, cancellationToken);
        return new AdminBranchMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "role"),
            AdminTranslationHelper.Get(ar, "role"),
            IdFormatter.ToStringId(item.BranchId),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminBranchMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminBranchMemberCreateDto(dto.NameEn, dto.NameAr, dto.RoleEn, dto.RoleAr, dto.BranchId, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminBranchMemberCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.BranchMember,
            id,
            new { name = dto.NameEn, role = dto.RoleEn },
            new { name = dto.NameAr, role = dto.RoleAr },
            cancellationToken);
}
