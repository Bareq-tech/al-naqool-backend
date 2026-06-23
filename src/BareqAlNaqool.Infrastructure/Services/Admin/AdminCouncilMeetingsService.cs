using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminCouncilMeetingsService(AppDbContext db) : IAdminCrudService<AdminCouncilMeetingDto, AdminCouncilMeetingCreateDto, AdminCouncilMeetingUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilMeetingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilMeetings.AsNoTracking().OrderByDescending(x => x.MeetingDate).ToListAsync(cancellationToken);
        var result = new List<AdminCouncilMeetingDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilMeetingDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilMeetings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilMeetingDto> CreateAsync(AdminCouncilMeetingCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilMeeting
        {
            MeetingDate = dto.MeetingDate,
            TimeValue = dto.Time,
            Decisions = dto.Decisions,
            Tasks = dto.Tasks,
            Attachments = dto.Attachments,
            IsLatest = dto.IsLatest,
            MinutesFileUrl = dto.MinutesFileUrl
        };
        db.CouncilMeetings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilMeetingDto?> UpdateAsync(string id, AdminCouncilMeetingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilMeetings.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.MeetingDate = dto.MeetingDate;
        entity.TimeValue = dto.Time;
        entity.Decisions = dto.Decisions;
        entity.Tasks = dto.Tasks;
        entity.Attachments = dto.Attachments;
        entity.IsLatest = dto.IsLatest;
        entity.MinutesFileUrl = dto.MinutesFileUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilMeetings.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilMeetings.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilMeetingDto> MapAsync(CouncilMeeting item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilMeeting, item.Id, cancellationToken);
        return new AdminCouncilMeetingDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "date"),
            AdminTranslationHelper.Get(ar, "date"),
            item.TimeValue,
            AdminTranslationHelper.Get(en, "location"),
            AdminTranslationHelper.Get(ar, "location"),
            item.Decisions,
            item.Tasks,
            item.Attachments,
            item.IsLatest,
            item.MinutesFileUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilMeetingUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilMeetingCreateDto(dto.TitleEn, dto.TitleAr, dto.DateEn, dto.DateAr, dto.Time, dto.LocationEn, dto.LocationAr, dto.Decisions, dto.Tasks, dto.Attachments, dto.IsLatest, dto.MinutesFileUrl, dto.MeetingDate), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilMeetingCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilMeeting,
            id,
            new { title = dto.TitleEn, date = dto.DateEn, time = dto.Time, location = dto.LocationEn },
            new { title = dto.TitleAr, date = dto.DateAr, time = dto.Time, location = dto.LocationAr },
            cancellationToken);
}
