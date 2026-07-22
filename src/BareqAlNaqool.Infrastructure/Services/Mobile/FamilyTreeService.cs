using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class FamilyTreeService(AppDbContext db, AppDataHelper helper) : IFamilyTreeService
{
    public async Task<IReadOnlyList<FamilyBranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        var branches = await db.FamilyBranches.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<FamilyBranchDto>();
        foreach (var branch in branches)
        {
            result.Add(await MapBranchAsync(branch, cancellationToken));
        }

        return result;
    }

    public async Task<FamilyBranchDto?> GetBranchByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var branch = await db.FamilyBranches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return branch is null ? null : await MapBranchAsync(branch, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchMemberDto>> GetBranchMembersAsync(string branchId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(branchId);
        var members = await db.BranchMembers.AsNoTracking().Where(x => x.BranchId == parsedId).ToListAsync(cancellationToken);
        var result = new List<BranchMemberDto>();
        foreach (var member in members)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.BranchMember, member.Id, helper.Lang, cancellationToken);
            result.Add(new BranchMemberDto(
                IdFormatter.ToStringId(member.Id),
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "role"),
                IdFormatter.ToStringId(member.BranchId),
                member.ImageUrl));
        }

        return result;
    }

    public async Task<IReadOnlyList<TreeMemberDto>> GetFounderLineageAsync(CancellationToken cancellationToken = default)
    {
        var members = await db.TreeMembers.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<TreeMemberDto>();
        foreach (var member in members)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.TreeMember, member.Id, helper.Lang, cancellationToken);
            result.Add(new TreeMemberDto(
                IdFormatter.ToStringId(member.Id),
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "subtitle"),
                member.Generation,
                member.IsFounder,
                member.ImageUrl,
                member.ParentId is null ? null : IdFormatter.ToStringId(member.ParentId.Value),
                member.SortOrder));
        }

        return result;
    }

    private async Task<FamilyBranchDto> MapBranchAsync(FamilyBranch branch, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, branch.Id, helper.Lang, cancellationToken);
        return new FamilyBranchDto(
            IdFormatter.ToStringId(branch.Id),
            TranslationStore.GetString(map, "name"),
            branch.MemberCount,
            TranslationStore.GetString(map, "description"),
            branch.ImageUrl);
    }
}
