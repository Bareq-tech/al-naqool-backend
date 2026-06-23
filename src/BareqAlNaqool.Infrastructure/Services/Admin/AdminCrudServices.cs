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
            CategoryKey = dto.Category,
            CategoryColorHex = dto.CategoryColorHex,
            ImageUrl = dto.ImageUrl,
            PublishedAt = DateTime.UtcNow
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

        entity.CategoryKey = dto.Category;
        entity.CategoryColorHex = dto.CategoryColorHex;
        entity.ImageUrl = dto.ImageUrl;
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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.NewsItem, item.Id, "en", cancellationToken);
        return new AdminNewsDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "body"),
            TranslationStore.GetString(map, "publishedDate"),
            item.CategoryColorHex,
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminNewsUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminNewsCreateDto(
            dto.Category, dto.Title, dto.Description, dto.Body, dto.PublishedDate, dto.CategoryColorHex, dto.ImageUrl),
            cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminNewsCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new
        {
            category = dto.Category,
            title = dto.Title,
            description = dto.Description,
            body = dto.Body,
            publishedDate = dto.PublishedDate
        };
        await TranslationStore.SaveAsync(db, EntityTypes.NewsItem, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.NewsItem, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
            EventDate = DateTime.UtcNow,
            TimeValue = dto.Time
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

        entity.TimeValue = dto.Time;
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

    private async Task<AdminEventDto> MapAsync(EventItem item, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.EventItem, item.Id, "en", cancellationToken);
        return new AdminEventDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "day"),
            TranslationStore.GetString(map, "month"),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "location"),
            TranslationStore.GetString(map, "time"),
            TranslationStore.GetString(map, "fullDate"),
            TranslationStore.GetString(map, "description"),
            TranslationStore.GetString(map, "organizer"));
    }

    private Task SaveTranslationsAsync(int id, AdminEventUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminEventCreateDto(
            dto.Day, dto.Month, dto.Title, dto.Location, dto.Time, dto.FullDate, dto.Description, dto.Organizer),
            cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminEventCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new
        {
            dto.Day,
            dto.Month,
            dto.Title,
            dto.Location,
            time = dto.Time,
            dto.FullDate,
            dto.Description,
            dto.Organizer
        };
        await TranslationStore.SaveAsync(db, EntityTypes.EventItem, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.EventItem, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, item.Id, "en", cancellationToken);
        return new AdminBranchDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "name"),
            item.MemberCount,
            TranslationStore.GetString(map, "description"),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminBranchUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminBranchCreateDto(dto.Name, dto.MemberCount, dto.Description, dto.ImageUrl), cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminBranchCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new { name = dto.Name, description = dto.Description };
        await TranslationStore.SaveAsync(db, EntityTypes.FamilyBranch, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.FamilyBranch, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
            GalleryTypeKey = "Albums"
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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Album, item.Id, "en", cancellationToken);
        return new AdminAlbumDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "title"),
            item.PhotoCount,
            item.ImageUrl,
            item.IsFeatured,
            TranslationStore.GetString(map, "description"));
    }

    private Task SaveTranslationsAsync(int id, AdminAlbumUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminAlbumCreateDto(dto.Title, dto.PhotoCount, dto.ImageUrl, dto.IsFeatured, dto.Description), cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminAlbumCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new { title = dto.Title, description = dto.Description };
        await TranslationStore.SaveAsync(db, EntityTypes.Album, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.Album, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
            DocumentDate = DateTime.UtcNow,
            FileSize = dto.FileSize
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
        entity.FileSize = dto.FileSize;
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

    private async Task<AdminDocumentDto> MapAsync(Document item, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Document, item.Id, "en", cancellationToken);
        return new AdminDocumentDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "title"),
            item.FileSize,
            TranslationStore.GetString(map, "date"),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "description"));
    }

    private Task SaveTranslationsAsync(int id, AdminDocumentUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminDocumentCreateDto(dto.Title, dto.FileSize, dto.Date, dto.Category, dto.Description), cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminDocumentCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new { title = dto.Title, date = dto.Date, category = dto.Category, description = dto.Description };
        await TranslationStore.SaveAsync(db, EntityTypes.Document, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.Document, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
        var users = await userManager.Users.ToListAsync(cancellationToken);
        foreach (var user in users.Where(x => !x.IsGuest))
        {
            db.UserNotifications.Add(new UserNotification
            {
                NotificationId = entity.Id,
                UserId = user.Id,
                IsRead = false
            });
        }

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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.NotificationItem, item.Id, "en", cancellationToken);
        return new AdminNotificationDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "body"),
            TranslationStore.GetString(map, "type"));
    }

    private Task SaveTranslationsAsync(int id, AdminNotificationUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminNotificationCreateDto(dto.Title, dto.Body, dto.Type), cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminNotificationCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new { title = dto.Title, body = dto.Body, type = dto.Type };
        await TranslationStore.SaveAsync(db, EntityTypes.NotificationItem, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.NotificationItem, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.CouncilListItem, item.Id, "en", cancellationToken);
        return new AdminCouncilItemDto(
            IdFormatter.ToStringId(item.Id),
            item.ModuleKey,
            TranslationStore.GetString(map, "title"),
            TranslationStore.GetString(map, "subtitle"),
            TranslationStore.GetString(map, "meta"),
            TranslationStore.GetString(map, "status", item.StatusKey ?? string.Empty));
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilItemUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilItemCreateDto(dto.ModuleId, dto.Title, dto.Subtitle, dto.Meta, dto.Status), cancellationToken);

    private async Task SaveTranslationsAsync(int id, AdminCouncilItemCreateDto dto, CancellationToken cancellationToken)
    {
        var data = new { title = dto.Title, subtitle = dto.Subtitle, meta = dto.Meta, status = dto.Status };
        await TranslationStore.SaveAsync(db, EntityTypes.CouncilListItem, id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.CouncilListItem, id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
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
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.DirectoryMember, item.Id, "en", cancellationToken);
        var branchMap = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, item.BranchId, "en", cancellationToken);
        return new AdminDirectoryMemberDto(
            IdFormatter.ToStringId(item.Id),
            TranslationStore.GetString(map, "name"),
            TranslationStore.GetString(map, "role"),
            IdFormatter.ToStringId(item.BranchId),
            TranslationStore.GetString(map, "branchName", TranslationStore.GetString(branchMap, "name")),
            item.Phone,
            item.Email,
            TranslationStore.GetString(map, "city"));
    }

    private Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(entity, new AdminDirectoryMemberCreateDto(dto.Name, dto.Role, dto.BranchId, dto.Phone, dto.Email, dto.City), cancellationToken);

    private async Task SaveTranslationsAsync(DirectoryMember entity, AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken)
    {
        var branchMap = await TranslationStore.GetMapAsync(db, EntityTypes.FamilyBranch, entity.BranchId, "en", cancellationToken);
        var data = new
        {
            name = dto.Name,
            role = dto.Role,
            branchName = TranslationStore.GetString(branchMap, "name"),
            city = dto.City
        };
        await TranslationStore.SaveAsync(db, EntityTypes.DirectoryMember, entity.Id, "en", data, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.DirectoryMember, entity.Id, "ar", data, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
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
            FullName = dto.FullName,
            DisplayRole = dto.Role,
            MemberId = dto.MemberId,
            Branch = dto.Branch,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        var role = dto.Role.Contains("Admin", StringComparison.OrdinalIgnoreCase) ? AppRoles.Admin : AppRoles.Member;
        await userManager.AddToRoleAsync(user, role);
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
        user.FullName = dto.FullName;
        user.DisplayRole = dto.Role;
        user.MemberId = dto.MemberId;
        user.Branch = dto.Branch;
        await userManager.UpdateAsync(user);
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.Password);
        }

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

    private async Task<AdminUserDto> MapAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AdminUserDto(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? AppRoles.Member,
            user.MemberId,
            user.Branch,
            user.IsGuest);
    }
}
