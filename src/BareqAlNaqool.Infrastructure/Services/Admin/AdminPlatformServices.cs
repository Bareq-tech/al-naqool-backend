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

public class AdminHomeService(AppDbContext db) : IAdminHomeService
{
    private const int SingletonId = 1;

    public async Task<AdminHomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await GetOrCreateAsync(cancellationToken);
        return Map(stats);
    }

    public async Task<AdminHomeStatsDto> UpdateHomeStatsAsync(AdminHomeStatsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var stats = await GetOrCreateAsync(cancellationToken);
        stats.TotalMembers = dto.TotalMembers;
        stats.FamilyBranches = dto.FamilyBranches;
        stats.NewsUpdates = dto.NewsUpdates;
        stats.UpcomingEvents = dto.UpcomingEvents;
        await db.SaveChangesAsync(cancellationToken);
        return Map(stats);
    }

    private async Task<HomeStat> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var stats = await db.HomeStats.FirstOrDefaultAsync(x => x.Id == SingletonId, cancellationToken);
        if (stats is not null)
        {
            return stats;
        }

        stats = new HomeStat { Id = SingletonId };
        db.HomeStats.Add(stats);
        await db.SaveChangesAsync(cancellationToken);
        return stats;
    }

    private static AdminHomeStatsDto Map(HomeStat stats) =>
        new(stats.TotalMembers, stats.FamilyBranches, stats.NewsUpdates, stats.UpcomingEvents);
}

public class AdminPresidentService(AppDbContext db) : IAdminPresidentService
{
    private const int EntityId = 1;

    public async Task<AdminPresidentDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.President, EntityId, cancellationToken);
        return new AdminPresidentDto(
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"));
    }

    public async Task<AdminPresidentDto> UpdateAsync(AdminPresidentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.President,
            EntityId,
            new { name = dto.NameEn },
            new { name = dto.NameAr },
            cancellationToken);
        return new AdminPresidentDto(dto.NameEn, dto.NameAr);
    }
}

public class AdminRegistrationService(UserManager<ApplicationUser> userManager) : IAdminRegistrationService
{
    public async Task<IReadOnlyList<AdminRegistrationDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Where(x => x.AccountStatus == AccountStatuses.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<bool> ApproveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.AccountStatus != AccountStatuses.Pending)
        {
            return false;
        }

        user.AccountStatus = AccountStatuses.Active;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        await userManager.AddToRoleAsync(user, AppRoles.Member);
        return true;
    }

    public async Task<bool> RejectAsync(string userId, AdminRegistrationDecisionDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.AccountStatus != AccountStatuses.Pending)
        {
            return false;
        }

        user.AccountStatus = AccountStatuses.Rejected;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private static AdminRegistrationDto Map(ApplicationUser user) =>
        new(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.PhoneNumber ?? string.Empty,
            user.DateOfBirth,
            user.RegistrationRelation ?? user.DisplayRole,
            user.CreatedAt);
}

public class AdminPasswordService(AppDbContext db, UserManager<ApplicationUser> userManager) : IAdminPasswordService
{
    public async Task<IReadOnlyList<AdminPasswordResetRequestDto>> GetPendingResetsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await db.PasswordResetRequests.AsNoTracking()
            .Where(x => !x.IsResolved)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
        return requests.Select(Map).ToList();
    }

    public async Task<bool> ResetPasswordAsync(string userId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        return await SetPasswordAsync(user, dto.NewPassword);
    }

    public async Task<bool> ResolveResetRequestAsync(string requestId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var request = await db.PasswordResetRequests.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(requestId), cancellationToken);
        if (request is null || request.IsResolved)
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return false;
        }

        if (!await SetPasswordAsync(user, dto.NewPassword))
        {
            return false;
        }

        request.IsResolved = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> SetPasswordAsync(ApplicationUser user, string newPassword)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    private static AdminPasswordResetRequestDto Map(PasswordResetRequest request) =>
        new(
            IdFormatter.ToStringId(request.Id),
            IdFormatter.ToStringId(request.UserId),
            request.Email,
            request.RequestedAt,
            request.IsResolved);
}

public class AdminRoleService(UserManager<ApplicationUser> userManager) : IAdminRoleService
{
    public Task<IReadOnlyList<AdminRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AdminRoleDto>>(AppRoles.All.Select(x => new AdminRoleDto(x)).ToList());

    public async Task<bool> AssignRoleAsync(string userId, AdminAssignRoleDto dto, CancellationToken cancellationToken = default)
    {
        if (!AppRoles.All.Contains(dto.Role))
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        var result = await userManager.AddToRoleAsync(user, dto.Role);
        if (result.Succeeded)
        {
            user.DisplayRole = dto.Role;
            await userManager.UpdateAsync(user);
        }

        return result.Succeeded;
    }
}

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

public class AdminMessagingService(AppDbContext db) : IAdminMessagingService
{
    public async Task<IReadOnlyList<AdminChatMessageDto>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var messages = await db.ChatMessages.AsNoTracking()
            .Where(x => x.ConversationId == parsedId)
            .OrderBy(x => x.SentAt)
            .ToListAsync(cancellationToken);

        var result = new List<AdminChatMessageDto>();
        foreach (var message in messages)
        {
            result.Add(await MapAsync(message, cancellationToken));
        }

        return result;
    }

    public async Task<AdminChatMessageDto> BroadcastAsync(AdminBroadcastMessageDto dto, int adminUserId, CancellationToken cancellationToken = default)
    {
        var conversationId = IdFormatter.ParseId(dto.ConversationId);
        var admin = await db.Users.AsNoTracking().FirstAsync(x => x.Id == adminUserId, cancellationToken);
        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderUserId = adminUserId,
            SenderNameKey = admin.FullName,
            IsAnnouncement = dto.IsAnnouncement,
            SentAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(message);
        var conversation = await db.Conversations.FirstAsync(x => x.Id == conversationId, cancellationToken);
        conversation.LastMessageAt = message.SentAt;
        await db.SaveChangesAsync(cancellationToken);

        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.ChatMessage,
            message.Id,
            new { content = dto.ContentEn, senderName = admin.FullName, time = TimeAgoFormatter.FormatTime(message.SentAt, "en") },
            new { content = dto.ContentAr, senderName = admin.FullName, time = TimeAgoFormatter.FormatTime(message.SentAt, "ar") },
            cancellationToken);

        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Conversation,
            conversationId,
            new { lastMessage = dto.ContentEn },
            new { lastMessage = dto.ContentAr },
            cancellationToken);

        return await MapAsync(message, cancellationToken);
    }

    public async Task<bool> HideMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var message = await db.ChatMessages.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(messageId), cancellationToken);
        if (message is null)
        {
            return false;
        }

        message.IsHidden = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var message = await db.ChatMessages.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(messageId), cancellationToken);
        if (message is null)
        {
            return false;
        }

        db.ChatMessages.Remove(message);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminChatMessageDto> MapAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.ChatMessage, message.Id, cancellationToken);
        var senderNameEn = AdminTranslationHelper.Get(en, "senderName");
        var senderNameAr = AdminTranslationHelper.Get(ar, "senderName");
        if (string.IsNullOrEmpty(senderNameEn))
        {
            senderNameEn = message.SenderNameKey;
        }

        if (string.IsNullOrEmpty(senderNameAr))
        {
            senderNameAr = message.SenderNameKey;
        }

        return new AdminChatMessageDto(
            IdFormatter.ToStringId(message.Id),
            IdFormatter.ToStringId(message.ConversationId),
            senderNameEn,
            senderNameAr,
            AdminTranslationHelper.Get(en, "content"),
            AdminTranslationHelper.Get(ar, "content"),
            message.IsAnnouncement,
            message.IsHidden,
            message.SentAt,
            message.SenderUserId.HasValue ? IdFormatter.ToStringId(message.SenderUserId.Value) : null);
    }
}

public class AdminContactService(AppDbContext db) : IAdminContactService
{
    public async Task<IReadOnlyList<AdminContactSubmissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.ContactSubmissions.AsNoTracking().OrderByDescending(x => x.SubmittedAt).ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<AdminContactSubmissionDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.ContactSubmissions.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AdminContactSubmissionDto Map(ContactSubmission item) =>
        new(
            IdFormatter.ToStringId(item.Id),
            item.Name,
            item.Email,
            item.Subject,
            item.Message,
            item.SubmittedAt,
            item.IsRead);
}

public class AdminSettingsService(AppDbContext db) : IAdminSettingsService
{
    private static readonly string[] KnownKeys =
    [
        AppSettingKeys.NewsCategories,
        AppSettingKeys.MessageFilters,
        AppSettingKeys.GalleryTypes,
        AppSettingKeys.DocumentCategories,
        AppSettingKeys.DirectoryBranches,
        AppSettingKeys.TermsAndConditions,
        AppSettingKeys.GuestPermissions,
        AppSettingKeys.AppBranding,
        AppSettingKeys.QuickAccessTiles
    ];

    public async Task<AdminAppSettingDto?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsKnownKey(key))
        {
            return null;
        }

        var setting = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        return setting is null ? null : new AdminAppSettingDto(setting.Key, setting.ValueJson);
    }

    public async Task<AdminAppSettingDto> UpdateSettingAsync(string key, AdminAppSettingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (!IsKnownKey(key))
        {
            throw new ArgumentException($"Unknown setting key: {key}", nameof(key));
        }

        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        setting.ValueJson = dto.ValueJson;
        await db.SaveChangesAsync(cancellationToken);
        return new AdminAppSettingDto(setting.Key, setting.ValueJson);
    }

    public async Task<AdminLocalizedListSettingDto> GetLocalizedListAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return new AdminLocalizedListSettingDto(
            key,
            ParseStringList(root, "en"),
            ParseStringList(root, "ar"));
    }

    public async Task<AdminLocalizedListSettingDto> UpdateLocalizedListAsync(string key, AdminLocalizedListSettingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new { en = dto.En, ar = dto.Ar });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminLocalizedListSettingDto(key, dto.En, dto.Ar);
    }

    public async Task<AdminTermsDto> GetTermsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.TermsAndConditions, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return new AdminTermsDto(
            root.TryGetProperty("en", out var en) ? en.GetString() ?? string.Empty : string.Empty,
            root.TryGetProperty("ar", out var ar) ? ar.GetString() ?? string.Empty : string.Empty);
    }

    public async Task<AdminTermsDto> UpdateTermsAsync(AdminTermsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.TermsAndConditions, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new { en = dto.En, ar = dto.Ar });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminTermsDto(dto.En, dto.Ar);
    }

    public async Task<AdminGuestPermissionsDto> GetGuestPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.GuestPermissions, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return ParseGuestPermissions(root);
    }

    public async Task<AdminGuestPermissionsDto> UpdateGuestPermissionsAsync(AdminGuestPermissionsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.GuestPermissions, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new
        {
            news = dto.News,
            events = dto.Events,
            gallery = dto.Gallery,
            documents = dto.Documents,
            council = dto.Council,
            messages = dto.Messages,
            directory = dto.Directory,
            familyTree = dto.FamilyTree
        });
        await db.SaveChangesAsync(cancellationToken);
        return MapGuestPermissions(dto);
    }

    public async Task<AdminBrandingDto> GetBrandingAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.AppBranding, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return ParseBranding(root);
    }

    public async Task<AdminBrandingDto> UpdateBrandingAsync(AdminBrandingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.AppBranding, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new
        {
            appNameEn = dto.AppNameEn,
            appNameAr = dto.AppNameAr,
            logoUrl = dto.LogoUrl,
            primaryColorHex = dto.PrimaryColorHex
        });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminBrandingDto(dto.AppNameEn, dto.AppNameAr, dto.LogoUrl, dto.PrimaryColorHex);
    }

    public async Task<IReadOnlyList<AdminQuickAccessTileDto>> GetQuickAccessTilesAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.QuickAccessTiles, cancellationToken);
        return ParseQuickAccessTiles(setting.ValueJson);
    }

    public async Task<IReadOnlyList<AdminQuickAccessTileDto>> UpdateQuickAccessTilesAsync(AdminQuickAccessTilesUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.QuickAccessTiles, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(dto.Tiles.Select(x => new
        {
            key = x.Key,
            labelEn = x.LabelEn,
            labelAr = x.LabelAr,
            iconName = x.IconName,
            route = x.Route,
            sortOrder = x.SortOrder
        }));
        await db.SaveChangesAsync(cancellationToken);
        return dto.Tiles;
    }

    private async Task<AppSetting> GetOrCreateSettingAsync(string key, CancellationToken cancellationToken)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is not null)
        {
            return setting;
        }

        setting = new AppSetting { Key = key, ValueJson = GetDefaultValueJson(key) };
        db.AppSettings.Add(setting);
        await db.SaveChangesAsync(cancellationToken);
        return setting;
    }

    private static bool IsKnownKey(string key) => KnownKeys.Contains(key);

    private static string GetDefaultValueJson(string key) => key switch
    {
        AppSettingKeys.TermsAndConditions => JsonSerializer.Serialize(new { en = string.Empty, ar = string.Empty }),
        AppSettingKeys.GuestPermissions => JsonSerializer.Serialize(new
        {
            news = true,
            events = true,
            gallery = true,
            documents = false,
            council = false,
            messages = false,
            directory = false,
            familyTree = true
        }),
        AppSettingKeys.AppBranding => JsonSerializer.Serialize(new
        {
            appNameEn = string.Empty,
            appNameAr = string.Empty,
            logoUrl = string.Empty,
            primaryColorHex = string.Empty
        }),
        AppSettingKeys.QuickAccessTiles => "[]",
        _ => JsonSerializer.Serialize(new { en = Array.Empty<string>(), ar = Array.Empty<string>() })
    };

    private static IReadOnlyList<string> ParseStringList(JsonElement root, string language)
    {
        if (!root.TryGetProperty(language, out var values) || values.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return values.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
    }

    private static AdminGuestPermissionsDto ParseGuestPermissions(JsonElement root) =>
        new(
            GetBool(root, "news"),
            GetBool(root, "events"),
            GetBool(root, "gallery"),
            GetBool(root, "documents"),
            GetBool(root, "council"),
            GetBool(root, "messages"),
            GetBool(root, "directory"),
            GetBool(root, "familyTree"));

    private static AdminGuestPermissionsDto MapGuestPermissions(AdminGuestPermissionsUpdateDto dto) =>
        new(dto.News, dto.Events, dto.Gallery, dto.Documents, dto.Council, dto.Messages, dto.Directory, dto.FamilyTree);

    private static AdminBrandingDto ParseBranding(JsonElement root) =>
        new(
            GetString(root, "appNameEn"),
            GetString(root, "appNameAr"),
            GetString(root, "logoUrl"),
            GetString(root, "primaryColorHex"));

    private static IReadOnlyList<AdminQuickAccessTileDto> ParseQuickAccessTiles(string valueJson)
    {
        var root = JsonDocument.Parse(valueJson).RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return root.EnumerateArray().Select(x => new AdminQuickAccessTileDto(
            GetString(x, "key"),
            GetString(x, "labelEn"),
            GetString(x, "labelAr"),
            GetString(x, "iconName"),
            GetString(x, "route"),
            x.TryGetProperty("sortOrder", out var sort) && sort.TryGetInt32(out var order) ? order : 0)).ToList();
    }

    private static string GetString(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    private static bool GetBool(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
}
