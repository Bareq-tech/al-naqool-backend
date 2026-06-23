using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class NotificationsService(AppDbContext db, AppDataHelper helper) : INotificationsService
{
    public async Task<IReadOnlyList<NotificationItemDto>> GetNotificationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await db.UserNotifications.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Notification)
            .OrderByDescending(x => x.Notification.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<NotificationItemDto>();
        foreach (var item in items)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.NotificationItem, item.NotificationId, helper.Lang, cancellationToken);
            result.Add(new NotificationItemDto(
                IdFormatter.ToStringId(item.NotificationId),
                TranslationStore.GetString(map, "title"),
                TranslationStore.GetString(map, "body"),
                TimeAgoFormatter.Format(item.Notification.CreatedAt, helper.Lang),
                TranslationStore.GetString(map, "type"),
                item.IsRead));
        }

        return result;
    }

    public async Task MarkAsReadAsync(string id, int userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(id);
        var item = await db.UserNotifications.FirstOrDefaultAsync(x => x.NotificationId == parsedId && x.UserId == userId, cancellationToken);
        if (item is not null)
        {
            item.IsRead = true;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await db.UserNotifications.Where(x => x.UserId == userId && !x.IsRead).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.IsRead = true;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
        => await db.UserNotifications.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
}
