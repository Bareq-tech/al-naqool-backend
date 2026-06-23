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

public class AdminNewsService(AppDbContext db) : IAdminCrudService<AdminNewsDto, AdminNewsCreateDto, AdminNewsUpdateDto>
{
    public async Task<IReadOnlyList<AdminNewsDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.NewsItems.AsNoTracking().OrderByDescending(x => x.PublishedAt).ToListAsync(cancellationToken);
        var result = new List<AdminNewsDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminNewsDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.NewsItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminNewsDto> CreateAsync(AdminNewsCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new NewsItem
        {
            CategoryKey = dto.CategoryEn,
            CategoryColorHex = dto.CategoryColorHex,
            ImageUrl = dto.ImageUrl,
            IsFeatured = dto.IsFeatured,
            PublishStatus = dto.PublishStatus,
            PublishedAt = dto.PublishedAt ?? DateTime.UtcNow
        };
        db.NewsItems.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminNewsDto?> UpdateAsync(string id, AdminNewsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.NewsItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CategoryKey = dto.CategoryEn;
        entity.CategoryColorHex = dto.CategoryColorHex;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsFeatured = dto.IsFeatured;
        entity.PublishStatus = dto.PublishStatus;
        if (dto.PublishedAt.HasValue)
        {
            entity.PublishedAt = dto.PublishedAt.Value;
        }

        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.NewsItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.NewsItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminNewsDto> MapAsync(NewsItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.NewsItem, item.Id, cancellationToken);
        return new AdminNewsDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "category"),
            AdminTranslationHelper.Get(ar, "category"),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            AdminTranslationHelper.Get(en, "body"),
            AdminTranslationHelper.Get(ar, "body"),
            AdminTranslationHelper.Get(en, "publishedDate"),
            AdminTranslationHelper.Get(ar, "publishedDate"),
            item.CategoryColorHex,
            item.ImageUrl,
            item.IsFeatured,
            item.PublishStatus,
            item.PublishedAt);
    }

    private Task SaveTranslationsAsync(int id, AdminNewsUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminNewsCreateDto(
            dto.CategoryEn, dto.CategoryAr, dto.TitleEn, dto.TitleAr, dto.DescriptionEn, dto.DescriptionAr,
            dto.BodyEn, dto.BodyAr, dto.PublishedDateEn, dto.PublishedDateAr, dto.CategoryColorHex, dto.ImageUrl,
            dto.IsFeatured, dto.PublishStatus, dto.PublishedAt), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminNewsCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.NewsItem,
            id,
            new { category = dto.CategoryEn, title = dto.TitleEn, description = dto.DescriptionEn, body = dto.BodyEn, publishedDate = dto.PublishedDateEn },
            new { category = dto.CategoryAr, title = dto.TitleAr, description = dto.DescriptionAr, body = dto.BodyAr, publishedDate = dto.PublishedDateAr },
            cancellationToken);
}

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

public class AdminAlbumsService(AppDbContext db) : IAdminCrudService<AdminAlbumDto, AdminAlbumCreateDto, AdminAlbumUpdateDto>
{
    public async Task<IReadOnlyList<AdminAlbumDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Albums.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminAlbumDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminAlbumDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Albums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminAlbumDto> CreateAsync(AdminAlbumCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Album
        {
            PhotoCount = dto.PhotoCount,
            ImageUrl = dto.ImageUrl,
            IsFeatured = dto.IsFeatured,
            GalleryTypeKey = dto.GalleryTypeKey
        };
        db.Albums.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminAlbumDto?> UpdateAsync(string id, AdminAlbumUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Albums.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.PhotoCount = dto.PhotoCount;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsFeatured = dto.IsFeatured;
        entity.GalleryTypeKey = dto.GalleryTypeKey;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Albums.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Albums.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminAlbumDto> MapAsync(Album item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.Album, item.Id, cancellationToken);
        return new AdminAlbumDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            item.PhotoCount,
            item.ImageUrl,
            item.IsFeatured,
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            item.GalleryTypeKey);
    }

    private Task SaveTranslationsAsync(int id, AdminAlbumUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminAlbumCreateDto(
            dto.TitleEn, dto.TitleAr, dto.PhotoCount, dto.ImageUrl, dto.IsFeatured, dto.DescriptionEn, dto.DescriptionAr, dto.GalleryTypeKey),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminAlbumCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Album,
            id,
            new { title = dto.TitleEn, description = dto.DescriptionEn },
            new { title = dto.TitleAr, description = dto.DescriptionAr },
            cancellationToken);
}

public class AdminDocumentsService(AppDbContext db) : IAdminCrudService<AdminDocumentDto, AdminDocumentCreateDto, AdminDocumentUpdateDto>
{
    public async Task<IReadOnlyList<AdminDocumentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Documents.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminDocumentDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminDocumentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminDocumentDto> CreateAsync(AdminDocumentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Document
        {
            CategoryKey = dto.Category,
            DocumentDate = ParseDocumentDate(dto.DateEn),
            FileSize = dto.FileSize,
            FileUrl = dto.FileUrl,
            AccessLevel = dto.AccessLevel
        };
        db.Documents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminDocumentDto?> UpdateAsync(string id, AdminDocumentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CategoryKey = dto.Category;
        entity.DocumentDate = ParseDocumentDate(dto.DateEn);
        entity.FileSize = dto.FileSize;
        entity.FileUrl = dto.FileUrl;
        entity.AccessLevel = dto.AccessLevel;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Documents.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DateTime ParseDocumentDate(string dateEn)
    {
        if (DateTime.TryParse(dateEn, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private async Task<AdminDocumentDto> MapAsync(Document item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.Document, item.Id, cancellationToken);
        return new AdminDocumentDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            item.FileSize,
            AdminTranslationHelper.Get(en, "date"),
            AdminTranslationHelper.Get(ar, "date"),
            item.CategoryKey,
            AdminTranslationHelper.Get(en, "description"),
            AdminTranslationHelper.Get(ar, "description"),
            item.FileUrl,
            item.AccessLevel);
    }

    private Task SaveTranslationsAsync(int id, AdminDocumentUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminDocumentCreateDto(
            dto.TitleEn, dto.TitleAr, dto.FileSize, dto.DateEn, dto.DateAr, dto.Category, dto.DescriptionEn, dto.DescriptionAr, dto.FileUrl, dto.AccessLevel),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminDocumentCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Document,
            id,
            new { title = dto.TitleEn, date = dto.DateEn, description = dto.DescriptionEn },
            new { title = dto.TitleAr, date = dto.DateAr, description = dto.DescriptionAr },
            cancellationToken);
}

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

public class AdminCouncilItemsService(AppDbContext db) : IAdminCrudService<AdminCouncilItemDto, AdminCouncilItemCreateDto, AdminCouncilItemUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilListItems.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminCouncilItemDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilListItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilItemDto> CreateAsync(AdminCouncilItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilListItem
        {
            ModuleKey = dto.ModuleId,
            StatusKey = dto.Status
        };
        db.CouncilListItems.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilItemDto?> UpdateAsync(string id, AdminCouncilItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilListItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleKey = dto.ModuleId;
        entity.StatusKey = dto.Status;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilListItems.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilListItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilItemDto> MapAsync(CouncilListItem item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilListItem, item.Id, cancellationToken);
        return new AdminCouncilItemDto(
            IdFormatter.ToStringId(item.Id),
            item.ModuleKey,
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            AdminTranslationHelper.Get(en, "meta"),
            AdminTranslationHelper.Get(ar, "meta"),
            item.StatusKey ?? AdminTranslationHelper.Get(en, "status"));
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilItemUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilItemCreateDto(
            dto.ModuleId, dto.TitleEn, dto.TitleAr, dto.SubtitleEn, dto.SubtitleAr, dto.MetaEn, dto.MetaAr, dto.Status),
            cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilItemCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilListItem,
            id,
            new { title = dto.TitleEn, subtitle = dto.SubtitleEn, meta = dto.MetaEn, status = dto.Status ?? string.Empty },
            new { title = dto.TitleAr, subtitle = dto.SubtitleAr, meta = dto.MetaAr, status = dto.Status ?? string.Empty },
            cancellationToken);
}

public class AdminDirectoryService(AppDbContext db) : IAdminCrudService<AdminDirectoryMemberDto, AdminDirectoryMemberCreateDto, AdminDirectoryMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminDirectoryMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).ToListAsync(cancellationToken);
        var result = new List<AdminDirectoryMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminDirectoryMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.DirectoryMembers.AsNoTracking().Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminDirectoryMemberDto> CreateAsync(AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new DirectoryMember
        {
            BranchId = IdFormatter.ParseId(dto.BranchId),
            Phone = dto.Phone,
            Email = dto.Email
        };
        db.DirectoryMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminDirectoryMemberDto?> UpdateAsync(string id, AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.DirectoryMembers.Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.BranchId = IdFormatter.ParseId(dto.BranchId);
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.DirectoryMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.DirectoryMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminDirectoryMemberDto> MapAsync(DirectoryMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.DirectoryMember, item.Id, cancellationToken);
        var (branchEn, branchAr) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.FamilyBranch, item.BranchId, cancellationToken);
        return new AdminDirectoryMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "role"),
            AdminTranslationHelper.Get(ar, "role"),
            IdFormatter.ToStringId(item.BranchId),
            AdminTranslationHelper.Get(branchEn, "name"),
            AdminTranslationHelper.Get(branchAr, "name"),
            item.Phone,
            item.Email,
            AdminTranslationHelper.Get(en, "city"),
            AdminTranslationHelper.Get(ar, "city"));
    }

    private Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(entity, new AdminDirectoryMemberCreateDto(
            dto.NameEn, dto.NameAr, dto.RoleEn, dto.RoleAr, dto.BranchId, dto.Phone, dto.Email, dto.CityEn, dto.CityAr),
            cancellationToken);

    private async Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken)
    {
        var (branchEn, branchAr) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.FamilyBranch, entity.BranchId, cancellationToken);
        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.DirectoryMember,
            entity.Id,
            new
            {
                name = dto.NameEn,
                role = dto.RoleEn,
                branchName = AdminTranslationHelper.Get(branchEn, "name"),
                city = dto.CityEn
            },
            new
            {
                name = dto.NameAr,
                role = dto.RoleAr,
                branchName = AdminTranslationHelper.Get(branchAr, "name"),
                city = dto.CityAr
            },
            cancellationToken);
    }
}

public class AdminUsersService(UserManager<ApplicationUser> userManager) : IAdminCrudService<AdminUserDto, AdminUserCreateDto, AdminUserUpdateDto>
{
    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users.ToListAsync(cancellationToken);
        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            result.Add(await MapAsync(user));
        }

        return result;
    }

    public async Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is null ? null : await MapAsync(user);
    }

    public async Task<AdminUserDto> CreateAsync(AdminUserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.Phone,
            FullName = dto.FullName,
            DisplayRole = dto.Role,
            MemberId = dto.MemberId,
            Branch = dto.Branch,
            DateOfBirth = dto.DateOfBirth,
            MaritalStatus = dto.MaritalStatus,
            ChildrenCount = dto.ChildrenCount,
            AccountStatus = dto.AccountStatus,
            IsGuest = dto.Role == AppRoles.Guest,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await AssignRoleAsync(user, dto.Role);
        return await MapAsync(user);
    }

    public async Task<AdminUserDto?> UpdateAsync(string id, AdminUserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        user.Email = dto.Email;
        user.PhoneNumber = dto.Phone;
        user.FullName = dto.FullName;
        user.DisplayRole = dto.Role;
        user.MemberId = dto.MemberId;
        user.Branch = dto.Branch;
        user.DateOfBirth = dto.DateOfBirth;
        user.MaritalStatus = dto.MaritalStatus;
        user.ChildrenCount = dto.ChildrenCount;
        user.AccountStatus = dto.AccountStatus;
        user.IsGuest = dto.Role == AppRoles.Guest;
        await userManager.UpdateAsync(user);
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.Password);
        }

        await AssignRoleAsync(user, dto.Role);
        return await MapAsync(user);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private async Task AssignRoleAsync(ApplicationUser user, string role)
    {
        if (!AppRoles.All.Contains(role))
        {
            throw new InvalidOperationException($"Invalid role: {role}");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, role);
    }

    private async Task<AdminUserDto> MapAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AdminUserDto(
            user.Id.ToString(CultureInfo.InvariantCulture),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? AppRoles.Member,
            user.MemberId,
            user.Branch,
            user.PhoneNumber ?? string.Empty,
            user.DateOfBirth,
            user.MaritalStatus,
            user.ChildrenCount,
            user.AccountStatus,
            user.RegistrationRelation,
            user.IsGuest,
            user.TermsAcceptedAt);
    }
}
