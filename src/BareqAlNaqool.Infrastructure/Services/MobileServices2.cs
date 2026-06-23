using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using BareqAlNaqool.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class MessagesService(AppDbContext db, AppDataHelper helper) : IMessagesService
{
    public async Task<IReadOnlyList<string>> GetFilterTypesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.MessageFilters, cancellationToken);

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string? type, int? userId, CancellationToken cancellationToken = default)
    {
        var conversations = await db.Conversations.AsNoTracking().OrderByDescending(x => x.LastMessageAt).ToListAsync(cancellationToken);
        var result = new List<ConversationDto>();
        foreach (var conversation in conversations)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.Conversation, conversation.Id, helper.Lang, cancellationToken);
            var conversationType = TranslationStore.GetString(map, "type");
            if (type is not null && !string.Equals(type, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(conversationType, type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var unread = 0;
            if (userId.HasValue)
            {
                unread = await db.ConversationParticipants.AsNoTracking()
                    .Where(x => x.ConversationId == conversation.Id && x.UserId == userId.Value)
                    .Select(x => x.UnreadCount)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            result.Add(new ConversationDto(
                IdFormatter.ToStringId(conversation.Id),
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "lastMessage"),
                TranslationStore.GetString(map, "time"),
                unread > 0 ? unread : TranslationStore.GetInt(map, "unreadCount"),
                conversation.IsGroup,
                conversationType));
        }

        return result;
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(string id, int? userId, CancellationToken cancellationToken = default)
    {
        var conversation = await db.Conversations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (conversation is null)
        {
            return null;
        }

        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Conversation, conversation.Id, helper.Lang, cancellationToken);
        var unread = 0;
        if (userId.HasValue)
        {
            unread = await db.ConversationParticipants.AsNoTracking()
                .Where(x => x.ConversationId == conversation.Id && x.UserId == userId.Value)
                .Select(x => x.UnreadCount)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ConversationDto(
            IdFormatter.ToStringId(conversation.Id),
            TranslationStore.GetString(map, "name"),
            TranslationStore.GetString(map, "lastMessage"),
            TranslationStore.GetString(map, "time"),
            unread,
            conversation.IsGroup,
            TranslationStore.GetString(map, "type"));
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetChatMessagesAsync(string conversationId, int? userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var messages = await db.ChatMessages.AsNoTracking().Where(x => x.ConversationId == parsedId).OrderBy(x => x.SentAt).ToListAsync(cancellationToken);
        var result = new List<ChatMessageDto>();
        foreach (var message in messages)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.ChatMessage, message.Id, helper.Lang, cancellationToken);
            var senderName = TranslationStore.GetString(map, "senderName");
            if (userId.HasValue && message.SenderUserId == userId.Value)
            {
                senderName = helper.Lang == "ar" ? "أنت" : "You";
            }

            result.Add(new ChatMessageDto(
                IdFormatter.ToStringId(message.Id),
                IdFormatter.ToStringId(message.ConversationId),
                senderName,
                TranslationStore.GetString(map, "content"),
                TimeAgoFormatter.FormatTime(message.SentAt, helper.Lang),
                userId.HasValue && message.SenderUserId == userId.Value,
                message.IsAnnouncement));
        }

        return result;
    }

    public async Task SendMessageAsync(string conversationId, string content, int userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId, cancellationToken);
        var message = new ChatMessage
        {
            ConversationId = parsedId,
            SenderUserId = userId,
            SenderNameKey = user.FullName,
            SentAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(message);
        var conversation = await db.Conversations.FirstAsync(x => x.Id == parsedId, cancellationToken);
        conversation.LastMessageAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var enData = new { content, senderName = "You", time = TimeAgoFormatter.FormatTime(message.SentAt, "en"), isMe = true };
        var arData = new { content, senderName = "أنت", time = TimeAgoFormatter.FormatTime(message.SentAt, "ar"), isMe = true };
        await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, message.Id, "en", enData, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, message.Id, "ar", arData, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public class GalleryService(AppDbContext db, AppDataHelper helper) : IGalleryService
{
    public async Task<IReadOnlyList<string>> GetGalleryTypesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.GalleryTypes, cancellationToken);

    public async Task<IReadOnlyList<AlbumItemDto>> GetAlbumsAsync(string? type, CancellationToken cancellationToken = default)
    {
        var albums = await db.Albums.AsNoTracking().OrderByDescending(x => x.IsFeatured).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AlbumItemDto>();
        foreach (var album in albums)
        {
            if (type is not null && !string.Equals(type, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(album.GalleryTypeKey, type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(await MapAlbumAsync(album, cancellationToken));
        }

        return result;
    }

    public async Task<AlbumItemDto?> GetAlbumByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var album = await db.Albums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return album is null ? null : await MapAlbumAsync(album, cancellationToken);
    }

    public async Task<IReadOnlyList<GalleryPhotoDto>> GetAlbumPhotosAsync(string albumId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(albumId);
        var photos = await db.GalleryPhotos.AsNoTracking().Where(x => x.AlbumId == parsedId).ToListAsync(cancellationToken);
        var result = new List<GalleryPhotoDto>();
        foreach (var photo in photos)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.GalleryPhoto, photo.Id, helper.Lang, cancellationToken);
            result.Add(new GalleryPhotoDto(
                IdFormatter.ToStringId(photo.Id),
                IdFormatter.ToStringId(photo.AlbumId),
                TranslationStore.GetString(map, "caption"),
                photo.ImageUrl));
        }

        return result;
    }

    private async Task<AlbumItemDto> MapAlbumAsync(Album album, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Album, album.Id, helper.Lang, cancellationToken);
        return new AlbumItemDto(
            IdFormatter.ToStringId(album.Id),
            TranslationStore.GetString(map, "title"),
            album.PhotoCount,
            album.ImageUrl,
            album.IsFeatured,
            TranslationStore.GetString(map, "description"));
    }
}

public class DocumentsService(AppDbContext db, AppDataHelper helper) : IDocumentsService
{
    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.DocumentCategories, cancellationToken);

    public async Task<IReadOnlyList<DocumentItemDto>> GetDocumentsAsync(string? category, CancellationToken cancellationToken = default)
    {
        var docs = await db.Documents.AsNoTracking().OrderByDescending(x => x.DocumentDate).ToListAsync(cancellationToken);
        var result = new List<DocumentItemDto>();
        foreach (var doc in docs)
        {
            var dto = await MapDocumentAsync(doc, cancellationToken);
            if (category is not null && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.Category, category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<DocumentItemDto?> GetDocumentByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return doc is null ? null : await MapDocumentAsync(doc, cancellationToken);
    }

    private async Task<DocumentItemDto> MapDocumentAsync(Document doc, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Document, doc.Id, helper.Lang, cancellationToken);
        return new DocumentItemDto(
            IdFormatter.ToStringId(doc.Id),
            TranslationStore.GetString(map, "title"),
            doc.FileSize,
            TranslationStore.GetString(map, "date"),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "description"));
    }
}

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

public class ProfileService(AppDbContext db) : IProfileService
{
    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId, cancellationToken);
        return MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstAsync(x => x.Id == userId, cancellationToken);
        user.FullName = request.Name;
        user.DisplayRole = request.Role;
        user.MemberId = request.MemberId;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.Branch = request.Branch;
        user.DateOfBirth = request.DateOfBirth;
        user.MaritalStatus = request.MaritalStatus;
        user.ChildrenCount = request.ChildrenCount;
        await db.SaveChangesAsync(cancellationToken);
        return MapProfile(user);
    }

    private static UserProfileDto MapProfile(ApplicationUser user) => new(
        user.FullName,
        user.DisplayRole,
        user.MemberId,
        user.Email ?? string.Empty,
        user.PhoneNumber ?? string.Empty,
        user.Branch,
        user.DateOfBirth,
        user.MaritalStatus,
        user.ChildrenCount);
}

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

public class ContactService(AppDbContext db) : IContactService
{
    public async Task SubmitContactAsync(ContactRequestDto request, CancellationToken cancellationToken = default)
    {
        db.ContactSubmissions.Add(new ContactSubmission
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            SubmittedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        return new DashboardStatsDto(
            await db.Users.CountAsync(cancellationToken),
            await db.NewsItems.CountAsync(cancellationToken),
            await db.Events.CountAsync(cancellationToken),
            await db.FamilyBranches.CountAsync(cancellationToken),
            await db.Albums.CountAsync(cancellationToken),
            await db.Documents.CountAsync(cancellationToken),
            await db.Notifications.CountAsync(cancellationToken),
            await db.DirectoryMembers.CountAsync(cancellationToken),
            await db.ContactSubmissions.CountAsync(cancellationToken));
    }
}
