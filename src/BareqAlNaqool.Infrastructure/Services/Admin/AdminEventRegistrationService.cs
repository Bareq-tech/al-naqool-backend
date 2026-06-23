using System.Text.Json;
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

public class AdminEventRegistrationService(AppDbContext db) : IAdminEventRegistrationService
{
    public async Task<IReadOnlyList<AdminEventRegistrationDto>> GetByEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var parsedEventId = IdFormatter.ParseId(eventId);
        var registrations = await db.EventRegistrations.AsNoTracking()
            .Where(x => x.EventId == parsedEventId)
            .OrderByDescending(x => x.RegisteredAt)
            .ToListAsync(cancellationToken);

        var eventTitleEn = string.Empty;
        var eventTitleAr = string.Empty;
        var eventExists = await db.Events.AsNoTracking().AnyAsync(x => x.Id == parsedEventId, cancellationToken);
        if (eventExists)
        {
            var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.EventItem, parsedEventId, cancellationToken);
            eventTitleEn = AdminTranslationHelper.Get(en, "title");
            eventTitleAr = AdminTranslationHelper.Get(ar, "title");
        }

        var eventTitle = string.IsNullOrEmpty(eventTitleEn) ? eventTitleAr : eventTitleEn;
        var result = new List<AdminEventRegistrationDto>();
        foreach (var registration in registrations)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == registration.UserId, cancellationToken);
            result.Add(new AdminEventRegistrationDto(
                IdFormatter.ToStringId(registration.Id),
                eventId,
                eventTitle,
                IdFormatter.ToStringId(registration.UserId),
                user?.FullName ?? string.Empty,
                user?.Email ?? string.Empty,
                registration.RegisteredAt));
        }

        return result;
    }

    public async Task<bool> CancelRegistrationAsync(string registrationId, CancellationToken cancellationToken = default)
    {
        var registration = await db.EventRegistrations.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(registrationId), cancellationToken);
        if (registration is null)
        {
            return false;
        }

        db.EventRegistrations.Remove(registration);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
