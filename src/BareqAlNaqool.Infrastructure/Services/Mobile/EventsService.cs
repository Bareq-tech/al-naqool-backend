using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class EventsService(AppDbContext db, AppDataHelper helper) : IEventsService
{
    public async Task<IReadOnlyList<EventItemDto>> GetEventsAsync(string filter, int? userId, CancellationToken cancellationToken = default)
    {
        var items = await db.Events.AsNoTracking()
            .Where(x => x.IsPublic)
            .OrderBy(x => x.EventDate)
            .ToListAsync(cancellationToken);
        var registrations = userId.HasValue
            ? await db.EventRegistrations.AsNoTracking().Where(x => x.UserId == userId.Value).Select(x => x.EventId).ToListAsync(cancellationToken)
            : [];

        var result = new List<EventItemDto>();
        foreach (var item in items)
        {
            var dto = await MapEventAsync(item, registrations.Contains(item.Id), cancellationToken);
            result.Add(dto);
        }

        return filter switch
        {
            "Upcoming" => result.Take(3).ToList(),
            "My Events" => result.Where(x => x.IsMine).ToList(),
            _ => result
        };
    }

    public async Task<EventItemDto?> GetEventByIdAsync(string id, int? userId, CancellationToken cancellationToken = default)
    {
        var item = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return null;
        }

        var isMine = userId.HasValue && await db.EventRegistrations.AnyAsync(x => x.EventId == item.Id && x.UserId == userId.Value, cancellationToken);
        return await MapEventAsync(item, isMine, cancellationToken);
    }

    public async Task<bool> IsRegisteredAsync(string eventId, int userId, CancellationToken cancellationToken = default)
        => await db.EventRegistrations.AnyAsync(x => x.EventId == IdFormatter.ParseId(eventId) && x.UserId == userId, cancellationToken);

    public async Task RegisterForEventAsync(string eventId, int userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(eventId);
        var exists = await db.EventRegistrations.AnyAsync(x => x.EventId == parsedId && x.UserId == userId, cancellationToken);
        if (!exists)
        {
            db.EventRegistrations.Add(new EventRegistration
            {
                EventId = parsedId,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<EventItemDto> MapEventAsync(EventItem item, bool isMine, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.EventItem, item.Id, helper.Lang, cancellationToken);
        return new EventItemDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "day"),
            TranslationStore.GetString(map, "month"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "location"),
            TranslationStore.GetString(map, "time"),
            TranslationStore.GetString(map, "fullDate"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "organizer"),
            isMine);
    }
}
