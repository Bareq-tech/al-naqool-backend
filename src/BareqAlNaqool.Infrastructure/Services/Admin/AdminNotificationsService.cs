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

public class AdminNotificationsService(AppDbContext db, UserManager<ApplicationUser> userManager) : IAdminCrudService<AdminNotificationDto, AdminNotificationCreateDto, AdminNotificationUpdateDto>
{
    public async Task<IReadOnlyList<AdminNotificationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Notifications.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        var result = new List<AdminNotificationDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminNotificationDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminNotificationDto> CreateAsync(AdminNotificationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new NotificationItem
        {
            TypeKey = dto.Type,
            CreatedAt = DateTime.UtcNow
        };
        db.Notifications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var users = await userManager.Users
            .Where(x => !x.IsGuest && x.AccountStatus == AccountStatuses.Active)
            .ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            db.UserNotifications.Add(new UserNotification
            {
                NotificationId = entity.Id,
                UserId = user.Id,
                IsRead = false
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminNotificationDto?> UpdateAsync(string id, AdminNotificationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Notifications.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.TypeKey = dto.Type;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Notifications.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Notifications.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminNotificationDto> MapAsync(NotificationItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.NotificationItem, item.Id, cancellationToken);
        return new AdminNotificationDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "body"),
            AdminTranslationHelper.Get(ar, "body"),
            item.TypeKey);
    }

    private Task SaveTranslationsAsync(int id, AdminNotificationUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminNotificationCreateDto(dto.TitleEn, dto.TitleAr, dto.BodyEn, dto.BodyAr, dto.Type), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminNotificationCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.NotificationItem,
            id,
            new { title = dto.TitleEn, body = dto.BodyEn },
            new { title = dto.TitleAr, body = dto.BodyAr },
            cancellationToken);
}
