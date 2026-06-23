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

public class AdminEventsService(AppDbContext db) : IAdminCrudService<AdminEventDto, AdminEventCreateDto, AdminEventUpdateDto>
{
    public async Task<IReadOnlyList<AdminEventDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Events.AsNoTracking().OrderBy(x => x.EventDate).ToListAsync(cancellationToken);
        var result = new List<AdminEventDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminEventDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminEventDto> CreateAsync(AdminEventCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new EventItem
        {
            EventDate = dto.EventDate,
            TimeValue = dto.Time,
            IsPublic = dto.IsPublic,
            OrganizerUserId = ParseOrganizerUserId(dto.OrganizerUserId),
            CommitteeKey = dto.CommitteeKey
        };
        db.Events.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminEventDto?> UpdateAsync(string id, AdminEventUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Events.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.EventDate = dto.EventDate;
        entity.TimeValue = dto.Time;
        entity.IsPublic = dto.IsPublic;
        entity.OrganizerUserId = ParseOrganizerUserId(dto.OrganizerUserId);
        entity.CommitteeKey = dto.CommitteeKey;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Events.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Events.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static int? ParseOrganizerUserId(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private async Task<AdminEventDto> MapAsync(EventItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.EventItem, item.Id, cancellationToken);
        return new AdminEventDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "day"),
            AdminTranslationHelper.Get(ar, "day"),
            AdminTranslationHelper.Get(en, "month"),
            AdminTranslationHelper.Get(ar, "month"),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "location"),
            AdminTranslationHelper.Get(ar, "location"),
            item.TimeValue,
            AdminTranslationHelper.Get(en, "fullDate"),
            AdminTranslationHelper.Get(ar, "fullDate"),
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            AdminTranslationHelper.Get(en, "organizer"),
            AdminTranslationHelper.Get(ar, "organizer"),
            item.IsPublic,
            item.OrganizerUserId?.ToString(CultureInfo.InvariantCulture),
            item.CommitteeKey,
            item.EventDate);
    }

    private Task SaveTranslationsAsync(int id, AdminEventUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminEventCreateDto(
            dto.DayEn, dto.DayAr, dto.MonthEn, dto.MonthAr, dto.TitleEn, dto.TitleAr, dto.LocationEn, dto.LocationAr,
            dto.Time, dto.FullDateEn, dto.FullDateAr, dto.DescriptionEn, dto.DescriptionAr, dto.OrganizerEn, dto.OrganizerAr,
            dto.IsPublic, dto.OrganizerUserId, dto.CommitteeKey, dto.EventDate), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminEventCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.EventItem,
            id,
            new { day = dto.DayEn, month = dto.MonthEn, title = dto.TitleEn, location = dto.LocationEn, time = dto.Time, fullDate = dto.FullDateEn, description = dto.DescriptionEn, organizer = dto.OrganizerEn },
            new { day = dto.DayAr, month = dto.MonthAr, title = dto.TitleAr, location = dto.LocationAr, time = dto.Time, fullDate = dto.FullDateAr, description = dto.DescriptionAr, organizer = dto.OrganizerAr },
            cancellationToken);
}
