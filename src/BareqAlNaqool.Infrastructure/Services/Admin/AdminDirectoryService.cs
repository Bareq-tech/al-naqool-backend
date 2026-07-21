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

public class AdminDirectoryService(AppDbContext db) : IAdminCrudService<AdminDirectoryMemberDto, AdminDirectoryMemberCreateDto, AdminDirectoryMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminDirectoryMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).ToListAsync(cancellationToken);
        var result = new List<AdminDirectoryMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminDirectoryMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminDirectoryMemberDto> CreateAsync(AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var branchId = IdFormatter.ParseId(dto.BranchId);
        await EnsureBranchExistsAsync(branchId, cancellationToken);

        var entity = new DirectoryMember
        {
            BranchId = branchId,
            Phone = dto.Phone,
            Email = dto.Email
        };
        db.DirectoryMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminDirectoryMemberDto?> UpdateAsync(string id, AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.DirectoryMembers.Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var branchId = IdFormatter.ParseId(dto.BranchId);
        await EnsureBranchExistsAsync(branchId, cancellationToken);

        entity.BranchId = branchId;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.DirectoryMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.DirectoryMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
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

    private async Task<AdminDirectoryMemberDto> MapAsync(DirectoryMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.DirectoryMember, item.Id, cancellationToken);
        // Always resolve live branch names so renames stay consistent.
        var (branchEn, branchAr) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.FamilyBranch, item.BranchId, cancellationToken);
        return new AdminDirectoryMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "role"),
            AdminTranslationHelper.Get(ar, "role"),
            IdFormatter.ToStringId(item.BranchId),
            AdminTranslationHelper.Get(branchEn, "name"),
            AdminTranslationHelper.Get(branchAr, "name"),
            item.Phone,
            item.Email,
            AdminTranslationHelper.Get(en, "city"),
            AdminTranslationHelper.Get(ar, "city"));
    }

    private Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(entity, new AdminDirectoryMemberCreateDto(
            dto.NameEn, dto.NameAr, dto.RoleEn, dto.RoleAr, dto.BranchId, dto.Phone, dto.Email, dto.CityEn, dto.CityAr),
            cancellationToken);

    private async Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken)
    {
        var (branchEn, branchAr) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.FamilyBranch, entity.BranchId, cancellationToken);
        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.DirectoryMember,
            entity.Id,
            new
            {
                name = dto.NameEn,
                role = dto.RoleEn,
                branchName = AdminTranslationHelper.Get(branchEn, "name"),
                city = dto.CityEn
            },
            new
            {
                name = dto.NameAr,
                role = dto.RoleAr,
                branchName = AdminTranslationHelper.Get(branchAr, "name"),
                city = dto.CityAr
            },
            cancellationToken);
    }
}
