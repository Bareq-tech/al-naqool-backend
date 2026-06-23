using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class DirectoryService(AppDbContext db, AppDataHelper helper) : IDirectoryService
{
    public async Task<IReadOnlyList<string>> GetBranchesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.DirectoryBranches, cancellationToken);

    public async Task<IReadOnlyList<DirectoryMemberDto>> GetMembersAsync(string? query, string? branchId, CancellationToken cancellationToken = default)
    {
        var members = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<DirectoryMemberDto>();
        foreach (var member in members)
        {
            var dto = await MapMemberAsync(member, cancellationToken);
            if (branchId is not null && !string.Equals(branchId, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.BranchId, branchId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var haystack = $"{dto.Name} {dto.Role} {dto.Email} {dto.City}";
                if (!haystack.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<DirectoryMemberDto?> GetMemberByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var member = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return member is null ? null : await MapMemberAsync(member, cancellationToken);
    }

    private async Task<DirectoryMemberDto> MapMemberAsync(DirectoryMember member, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.DirectoryMember, member.Id, helper.Lang, cancellationToken);
        var branchMap = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, member.BranchId, helper.Lang, cancellationToken);
        return new DirectoryMemberDto(
            IdFormatter.ToStringId(member.Id),
            TranslationStore.GetString(map, "name"),
            TranslationStore.GetString(map, "role"),
            IdFormatter.ToStringId(member.BranchId),
            TranslationStore.GetString(map, "branchName", TranslationStore.GetString(branchMap, "name")),
            member.Phone,
            member.Email,
            TranslationStore.GetString(map, "city"));
    }
}
