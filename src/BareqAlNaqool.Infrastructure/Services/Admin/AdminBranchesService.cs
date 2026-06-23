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

public class AdminBranchesService(AppDbContext db) : IAdminCrudService<AdminBranchDto, AdminBranchCreateDto, AdminBranchUpdateDto>
{
    public async Task<IReadOnlyList<AdminBranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.FamilyBranches.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminBranchDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminBranchDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.FamilyBranches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminBranchDto> CreateAsync(AdminBranchCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new FamilyBranch
        {
            MemberCount = dto.MemberCount,
            ImageUrl = dto.ImageUrl
        };
        db.FamilyBranches.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminBranchDto?> UpdateAsync(string id, AdminBranchUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.FamilyBranches.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.MemberCount = dto.MemberCount;
        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.FamilyBranches.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.FamilyBranches.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminBranchDto> MapAsync(FamilyBranch item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.FamilyBranch, item.Id, cancellationToken);
        return new AdminBranchDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            item.MemberCount,
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminBranchUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminBranchCreateDto(dto.NameEn, dto.NameAr, dto.MemberCount, dto.DescriptionEn, dto.DescriptionAr, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminBranchCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.FamilyBranch,
            id,
            new { name = dto.NameEn, description = dto.DescriptionEn },
            new { name = dto.NameAr, description = dto.DescriptionAr },
            cancellationToken);
}
