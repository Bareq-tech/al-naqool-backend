using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class CouncilService(AppDbContext db, AppDataHelper helper) : ICouncilService
{
    public async Task<IReadOnlyList<CouncilModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var modules = await db.CouncilModules.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<CouncilModuleDto>();
        foreach (var module in modules)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.CouncilModule, module.Id, helper.Lang, cancellationToken);
            result.Add(new CouncilModuleDto(
                TranslationStore.GetString(map, "id"),
                module.IconName,
                TranslationStore.GetString(map, "label"),
                TranslationStore.GetString(map, "subtitle")));
        }

        return result;
    }

    public async Task<MeetingInfoDto> GetLatestMeetingAsync(CancellationToken cancellationToken = default)
    {
        var meeting = await db.CouncilMeetings.AsNoTracking().FirstAsync(x => x.IsLatest, cancellationToken);
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.CouncilMeeting, meeting.Id, helper.Lang, cancellationToken);
        return new MeetingInfoDto(
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "date"),
            TranslationStore.GetString(map, "time"),
            TranslationStore.GetString(map, "location"),
            meeting.Decisions,
            meeting.Tasks,
            meeting.Attachments);
    }

    public async Task<string> GetPresidentNameAsync(CancellationToken cancellationToken = default)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.President, 1, helper.Lang, cancellationToken);
        return TranslationStore.GetString(map, "name");
    }

    public async Task<IReadOnlyList<CouncilListItemDto>> GetModuleItemsAsync(string moduleId, CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilListItems.AsNoTracking().Where(x => x.ModuleKey == moduleId).ToListAsync(cancellationToken);
        var result = new List<CouncilListItemDto>();
        foreach (var item in items)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.CouncilListItem, item.Id, helper.Lang, cancellationToken);
            result.Add(new CouncilListItemDto(
                IdFormatter.ToStringId(item.Id),
                TranslationStore.GetString(map, "title"),
                TranslationStore.GetString(map, "subtitle"),
                TranslationStore.GetString(map, "meta"),
                TranslationStore.GetString(map, "status", item.StatusKey ?? string.Empty)));
        }

        return result;
    }
}
