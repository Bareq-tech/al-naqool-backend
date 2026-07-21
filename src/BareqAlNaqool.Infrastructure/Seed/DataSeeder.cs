using System.Globalization;
using System.Text.Json;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BareqAlNaqool.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (await IsSeedCompleteAsync(db))
        {
            return;
        }

        var seedPath = ResolveSeedPath(environment);
        var en = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(seedPath, "en.json"))).RootElement;
        var ar = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(seedPath, "ar.json"))).RootElement;

        await SeedRolesAsync(roleManager);

        var branchIds = await SeedBranchesAsync(db, en, ar);
        var userIds = await SeedUsersAsync(userManager, en, branchIds);
        await SeedPresidentAsync(db, en, ar);
        await SeedHomeStatsAsync(db, en);
        await SeedLandingSlidesAsync(db, en, ar);
        await SeedAppSettingsAsync(db, en, ar);
        var eventIds = await SeedEventsAsync(db, en, ar, userIds);
        await SeedEventRegistrationsAsync(db, en, eventIds, userIds);
        await SeedBranchMembersAsync(db, en, ar, branchIds);
        await SeedTreeMembersAsync(db, en, ar);
        var conversationIds = await SeedConversationsAsync(db, en, ar, userIds);
        await SeedChatMessagesAsync(db, en, ar, conversationIds, userIds);
        var albumIds = await SeedAlbumsAsync(db, en, ar);
        await SeedGalleryPhotosAsync(db, en, ar, albumIds);
        await SeedDocumentsAsync(db, en, ar);
        await SeedCouncilModulesAsync(db, en, ar);
        await SeedCouncilMeetingAsync(db, en, ar);
        await SeedCouncilItemsAsync(db, en, ar);
        await SeedNewsAsync(db, en, ar);
        await SeedNotificationsAsync(db, en, ar, userIds);
        await SeedDirectoryAsync(db, en, ar, branchIds, userIds);
        await SeedContactSubmissionsAsync(db, en);
        await SeedPasswordResetRequestsAsync(db, en, userIds);

        await db.SaveChangesAsync();
    }

    private static string ResolveSeedPath(IHostEnvironment environment)
    {
        var seedPath = Path.Combine(environment.ContentRootPath, "Seed");
        return Directory.Exists(seedPath) ? seedPath : Path.Combine(AppContext.BaseDirectory, "Seed");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
    }

    private static async Task<Dictionary<string, int>> SeedBranchesAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        foreach (var item in en.GetProperty("branches").EnumerateArray())
        {
            var key = item.GetProperty("key").GetString() ?? $"branch-{index}";
            var entity = new FamilyBranch
            {
                MemberCount = item.GetProperty("memberCount").GetInt32(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.FamilyBranches.Add(entity);
            await db.SaveChangesAsync();
            ids[key] = entity.Id;
            await SaveBilingualAsync(db, EntityTypes.FamilyBranch, entity.Id, item, ar.GetProperty("branches")[index]);
            index++;
        }

        return ids;
    }

    private static async Task<Dictionary<string, int>> SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        JsonElement en,
        IReadOnlyDictionary<string, int> branchIds)
    {
        var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in en.GetProperty("users").EnumerateArray())
        {
            var key = item.GetProperty("key").GetString() ?? string.Empty;
            var userName = item.GetProperty("userName").GetString() ?? string.Empty;
            var existing = await userManager.FindByNameAsync(userName);
            if (existing is not null)
            {
                ids[key] = existing.Id;
                continue;
            }

            var branchKey = item.GetProperty("branchKey").GetString() ?? string.Empty;
            branchIds.TryGetValue(branchKey, out var branchId);

            var user = new ApplicationUser
            {
                UserName = item.GetProperty("userName").GetString() ?? string.Empty,
                Email = item.GetProperty("email").GetString() ?? string.Empty,
                FullName = item.GetProperty("fullName").GetString() ?? string.Empty,
                DisplayRole = item.GetProperty("displayRole").GetString() ?? string.Empty,
                MemberId = item.GetProperty("memberId").GetString() ?? string.Empty,
                PhoneNumber = item.GetProperty("phone").GetString(),
                Branch = branchKey,
                DateOfBirth = item.GetProperty("dateOfBirth").GetString() ?? string.Empty,
                MaritalStatus = item.GetProperty("maritalStatus").GetString() ?? string.Empty,
                ChildrenCount = item.GetProperty("childrenCount").GetInt32(),
                AccountStatus = item.GetProperty("accountStatus").GetString() ?? AccountStatuses.Active,
                RegistrationRelation = item.TryGetProperty("registrationRelation", out var rel) ? rel.GetString() : null,
                IsGuest = false,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var result = await userManager.CreateAsync(user, item.GetProperty("password").GetString() ?? "password123");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user '{key}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }

            var role = item.GetProperty("role").GetString() ?? AppRoles.Member;
            await userManager.AddToRoleAsync(user, role);
            ids[key] = user.Id;
        }

        return ids;
    }

    private static async Task SeedPresidentAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        await TranslationStore.SaveAsync(db, EntityTypes.President, 1, "en", new { name = en.GetProperty("president").GetString() });
        await TranslationStore.SaveAsync(db, EntityTypes.President, 1, "ar", new { name = ar.GetProperty("president").GetString() });
    }

    private static async Task SeedHomeStatsAsync(AppDbContext db, JsonElement en)
    {
        if (await db.HomeStats.AnyAsync())
        {
            return;
        }

        var stats = en.GetProperty("homeStats");
        db.HomeStats.Add(new HomeStat
        {
            TotalMembers = stats.GetProperty("totalMembers").GetInt32(),
            FamilyBranches = stats.GetProperty("familyBranches").GetInt32(),
            NewsUpdates = stats.GetProperty("newsUpdates").GetInt32(),
            UpcomingEvents = stats.GetProperty("upcomingEvents").GetInt32()
        });
    }

    private static async Task SeedLandingSlidesAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var index = 0;
        foreach (var slide in en.GetProperty("landingSlides").EnumerateArray())
        {
            index++;
            var entity = new LandingSlide { SortOrder = index };
            db.LandingSlides.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.LandingSlide, entity.Id, slide, ar.GetProperty("landingSlides")[index - 1]);
        }
    }

    private static async Task SeedAppSettingsAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        if (await db.AppSettings.AnyAsync())
        {
            return;
        }

        SaveAppSetting(db, AppSettingKeys.NewsCategories, en, ar, "newsCategories");
        SaveAppSetting(db, AppSettingKeys.MessageFilters, en, ar, "messageFilters");
        SaveAppSetting(db, AppSettingKeys.GalleryTypes, en, ar, "galleryTypes");
        SaveAppSetting(db, AppSettingKeys.DocumentCategories, en, ar, "documentCategories");
        SaveAppSetting(db, AppSettingKeys.DirectoryBranches, en, ar, "directoryBranches");

        db.AppSettings.Add(new AppSetting
        {
            Key = AppSettingKeys.TermsAndConditions,
            ValueJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["en"] = en.GetProperty("termsEn").GetString() ?? string.Empty,
                ["ar"] = ar.GetProperty("termsAr").GetString() ?? string.Empty
            })
        });

        db.AppSettings.Add(new AppSetting
        {
            Key = AppSettingKeys.GuestPermissions,
            ValueJson = JsonSerializer.Serialize(new
            {
                news = true,
                events = true,
                gallery = true,
                documents = false,
                council = false,
                messages = false,
                directory = true,
                familyTree = true
            })
        });

        db.AppSettings.Add(new AppSetting
        {
            Key = AppSettingKeys.AppBranding,
            ValueJson = JsonSerializer.Serialize(new
            {
                appNameEn = "Bareq Al Naqool",
                appNameAr = "بريق النقول",
                logoUrl = string.Empty,
                primaryColorHex = "0x1B4D3E"
            })
        });

        db.AppSettings.Add(new AppSetting
        {
            Key = AppSettingKeys.QuickAccessTiles,
            ValueJson = JsonSerializer.Serialize(new[]
            {
                new { key = "news", labelEn = "News", labelAr = "الأخبار", iconName = "newspaper", route = "/news", sortOrder = 1 },
                new { key = "events", labelEn = "Events", labelAr = "الفعاليات", iconName = "calendar", route = "/events", sortOrder = 2 },
                new { key = "gallery", labelEn = "Gallery", labelAr = "المعرض", iconName = "photo", route = "/gallery", sortOrder = 3 },
                new { key = "council", labelEn = "Council", labelAr = "المجلس", iconName = "account_balance", route = "/council", sortOrder = 4 }
            })
        });
    }

    private static async Task<List<int>> SeedEventsAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> userIds)
    {
        var eventIds = new List<int>();
        var index = 0;
        foreach (var item in en.GetProperty("events").EnumerateArray())
        {
            int? organizerId = null;
            if (item.TryGetProperty("organizerUserKey", out var orgKey)
                && orgKey.GetString() is { } orgKeyValue
                && userIds.TryGetValue(orgKeyValue, out var parsedOrgId))
            {
                organizerId = parsedOrgId;
            }

            var entity = new EventItem
            {
                EventDate = ParseDate(item.GetProperty("fullDate").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                TimeValue = item.GetProperty("time").GetString() ?? string.Empty,
                OrganizerUserId = organizerId,
                CommitteeKey = item.TryGetProperty("committeeKey", out var ck) ? ck.GetString() : null,
                IsPublic = !item.TryGetProperty("isPublic", out var pub) || pub.GetBoolean()
            };
            db.Events.Add(entity);
            await db.SaveChangesAsync();
            eventIds.Add(entity.Id);
            await SaveBilingualAsync(db, EntityTypes.EventItem, entity.Id, item, ar.GetProperty("events")[index]);
            index++;
        }

        return eventIds;
    }

    private static Task SeedEventRegistrationsAsync(
        AppDbContext db,
        JsonElement en,
        IReadOnlyList<int> eventIds,
        IReadOnlyDictionary<string, int> userIds)
    {
        if (!userIds.TryGetValue("ahmed", out var ahmedId))
        {
            return Task.CompletedTask;
        }

        var eventIndex = 0;
        foreach (var item in en.GetProperty("events").EnumerateArray())
        {
            if (item.TryGetProperty("isMine", out var isMine) && isMine.GetBoolean() && eventIndex < eventIds.Count)
            {
                db.EventRegistrations.Add(new EventRegistration
                {
                    EventId = eventIds[eventIndex],
                    UserId = ahmedId,
                    RegisteredAt = DateTime.UtcNow.AddDays(-eventIndex)
                });
            }

            eventIndex++;
        }

        return Task.CompletedTask;
    }

    private static async Task SeedBranchMembersAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> branchIds)
    {
        var index = 0;
        foreach (var item in en.GetProperty("branchMembers").EnumerateArray())
        {
            var branchKey = item.GetProperty("branchKey").GetString() ?? string.Empty;
            var entity = new BranchMember
            {
                BranchId = branchIds[branchKey],
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.BranchMembers.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.BranchMember, entity.Id, item, ar.GetProperty("branchMembers")[index]);
            index++;
        }
    }

    private static async Task SeedTreeMembersAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var index = 0;
        var lastIdByGeneration = new Dictionary<int, int>();
        foreach (var item in en.GetProperty("founderLineage").EnumerateArray())
        {
            index++;
            var generation = item.GetProperty("generation").GetInt32();
            int? parentId = null;
            if (generation > 0 && lastIdByGeneration.TryGetValue(generation - 1, out var parentCandidate))
            {
                parentId = parentCandidate;
            }

            var entity = new TreeMember
            {
                ParentId = parentId,
                Generation = generation,
                IsFounder = item.TryGetProperty("isFounder", out var founder) && founder.GetBoolean(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                SortOrder = index
            };
            db.TreeMembers.Add(entity);
            await db.SaveChangesAsync();
            lastIdByGeneration[generation] = entity.Id;
            await SaveBilingualAsync(db, EntityTypes.TreeMember, entity.Id, item, ar.GetProperty("founderLineage")[index - 1]);
        }
    }

    private static async Task<Dictionary<string, int>> SeedConversationsAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> userIds)
    {
        var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        foreach (var item in en.GetProperty("conversations").EnumerateArray())
        {
            var key = item.GetProperty("key").GetString() ?? $"conv-{index}";
            var entity = new Conversation
            {
                IsGroup = item.GetProperty("isGroup").GetBoolean(),
                TypeKey = item.GetProperty("type").GetString() ?? string.Empty,
                LastMessageAt = DateTime.UtcNow.AddHours(-index - 1)
            };
            db.Conversations.Add(entity);
            await db.SaveChangesAsync();
            ids[key] = entity.Id;

            if (item.TryGetProperty("participantUserKeys", out var participants))
            {
                foreach (var participantKey in participants.EnumerateArray())
                {
                    var userKey = participantKey.GetString();
                    if (userKey is not null && userIds.TryGetValue(userKey, out var userId))
                    {
                        var unread = userKey == "ahmed" && item.TryGetProperty("unreadCount", out var uc) ? uc.GetInt32() : 0;
                        db.ConversationParticipants.Add(new ConversationParticipant
                        {
                            ConversationId = entity.Id,
                            UserId = userId,
                            UnreadCount = unread
                        });
                    }
                }
            }

            await SaveBilingualAsync(db, EntityTypes.Conversation, entity.Id, item, ar.GetProperty("conversations")[index]);
            index++;
        }

        return ids;
    }

    private static async Task SeedChatMessagesAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> conversationIds,
        IReadOnlyDictionary<string, int> userIds)
    {
        var index = 0;
        foreach (var item in en.GetProperty("chatMessages").EnumerateArray())
        {
            var conversationKey = item.GetProperty("conversationKey").GetString() ?? string.Empty;
            int? senderUserId = null;
            if (item.TryGetProperty("senderUserKey", out var senderKey)
                && senderKey.ValueKind != JsonValueKind.Null
                && senderKey.GetString() is { } senderKeyValue
                && userIds.TryGetValue(senderKeyValue, out var parsedSenderId))
            {
                senderUserId = parsedSenderId;
            }

            var entity = new ChatMessage
            {
                ConversationId = conversationIds[conversationKey],
                SenderUserId = senderUserId,
                SenderNameKey = item.GetProperty("senderName").GetString() ?? string.Empty,
                IsAnnouncement = item.TryGetProperty("isAnnouncement", out var ann) && ann.GetBoolean(),
                SentAt = DateTime.UtcNow.AddHours(-index - 1)
            };
            db.ChatMessages.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.ChatMessage, entity.Id, item, ar.GetProperty("chatMessages")[index]);
            index++;
        }
    }

    private static async Task<Dictionary<string, int>> SeedAlbumsAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        foreach (var item in en.GetProperty("albums").EnumerateArray())
        {
            var key = item.GetProperty("key").GetString() ?? $"album-{index}";
            var entity = new Album
            {
                PhotoCount = item.GetProperty("photoCount").GetInt32(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                IsFeatured = item.GetProperty("isFeatured").GetBoolean(),
                GalleryTypeKey = item.GetProperty("galleryTypeKey").GetString() ?? "Albums"
            };
            db.Albums.Add(entity);
            await db.SaveChangesAsync();
            ids[key] = entity.Id;
            await SaveBilingualAsync(db, EntityTypes.Album, entity.Id, item, ar.GetProperty("albums")[index]);
            index++;
        }

        return ids;
    }

    private static async Task SeedGalleryPhotosAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> albumIds)
    {
        var index = 0;
        foreach (var item in en.GetProperty("galleryPhotos").EnumerateArray())
        {
            var albumKey = item.GetProperty("albumKey").GetString() ?? string.Empty;
            var entity = new GalleryPhoto
            {
                AlbumId = albumIds[albumKey],
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.GalleryPhotos.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.GalleryPhoto, entity.Id, item, ar.GetProperty("galleryPhotos")[index]);
            index++;
        }
    }

    private static async Task SeedDocumentsAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var index = 0;
        foreach (var item in en.GetProperty("documents").EnumerateArray())
        {
            var entity = new Document
            {
                CategoryKey = item.GetProperty("category").GetString() ?? string.Empty,
                DocumentDate = ParseDate(item.GetProperty("date").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                FileSize = item.GetProperty("fileSize").GetString() ?? string.Empty,
                FileUrl = item.TryGetProperty("fileUrl", out var url) ? url.GetString() ?? string.Empty : string.Empty,
                AccessLevel = item.TryGetProperty("accessLevel", out var level) ? level.GetString() ?? AccessLevels.Public : AccessLevels.Public
            };
            db.Documents.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.Document, entity.Id, item, ar.GetProperty("documents")[index]);
            index++;
        }
    }

    private static async Task SeedCouncilModulesAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var index = 0;
        foreach (var item in en.GetProperty("councilModules").EnumerateArray())
        {
            index++;
            var entity = new CouncilModule
            {
                ModuleKey = item.GetProperty("id").GetString() ?? string.Empty,
                IconName = item.GetProperty("iconName").GetString() ?? string.Empty,
                SortOrder = index
            };
            db.CouncilModules.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.CouncilModule, entity.Id, item, ar.GetProperty("councilModules")[index - 1]);
        }
    }

    private static async Task SeedCouncilMeetingAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var meeting = en.GetProperty("latestMeeting");
        var meetingEntity = new CouncilMeeting
        {
            MeetingDate = ParseDate(meeting.GetProperty("date").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            TimeValue = meeting.GetProperty("time").GetString() ?? string.Empty,
            Decisions = meeting.GetProperty("decisions").GetInt32(),
            Tasks = meeting.GetProperty("tasks").GetInt32(),
            Attachments = meeting.GetProperty("attachments").GetInt32(),
            IsLatest = true,
            MinutesFileUrl = meeting.TryGetProperty("minutesFileUrl", out var url) ? url.GetString() : null
        };
        db.CouncilMeetings.Add(meetingEntity);
        await db.SaveChangesAsync();
        await SaveBilingualAsync(db, EntityTypes.CouncilMeeting, meetingEntity.Id, meeting, ar.GetProperty("latestMeeting"));
    }

    private static async Task SeedCouncilItemsAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        foreach (var moduleKey in new[] { "meetings", "committees", "voting", "tasks", "decisions", "members" })
        {
            var listIndex = 0;
            foreach (var item in en.GetProperty("councilItems").GetProperty(moduleKey).EnumerateArray())
            {
                var entity = new CouncilListItem
                {
                    ModuleKey = moduleKey,
                    StatusKey = item.TryGetProperty("status", out var status) ? status.GetString() : null
                };
                db.CouncilListItems.Add(entity);
                await db.SaveChangesAsync();
                await SaveBilingualAsync(db, EntityTypes.CouncilListItem, entity.Id, item, ar.GetProperty("councilItems").GetProperty(moduleKey)[listIndex]);
                listIndex++;
            }
        }
    }

    private static async Task SeedNewsAsync(AppDbContext db, JsonElement en, JsonElement ar)
    {
        var index = 0;
        foreach (var item in en.GetProperty("newsItems").EnumerateArray())
        {
            var entity = new NewsItem
            {
                CategoryKey = item.GetProperty("category").GetString() ?? string.Empty,
                CategoryColorHex = ParseColorHex(item.GetProperty("categoryColorHex").GetString() ?? "0"),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                IsFeatured = item.TryGetProperty("isFeatured", out var featured) && featured.GetBoolean(),
                PublishStatus = item.TryGetProperty("publishStatus", out var status) ? status.GetString() ?? PublishStatuses.Published : PublishStatuses.Published,
                PublishedAt = ParseDate(item.GetProperty("publishedDate").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
            };
            db.NewsItems.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.NewsItem, entity.Id, item, ar.GetProperty("newsItems")[index]);
            index++;
        }
    }

    private static async Task SeedNotificationsAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> userIds)
    {
        if (!userIds.TryGetValue("ahmed", out var ahmedId))
        {
            return;
        }

        var index = 0;
        foreach (var item in en.GetProperty("notifications").EnumerateArray())
        {
            var entity = new NotificationItem
            {
                TypeKey = item.GetProperty("type").GetString() ?? string.Empty,
                CreatedAt = DateTime.UtcNow.AddHours(-(index + 1) * 5)
            };
            db.Notifications.Add(entity);
            await db.SaveChangesAsync();
            db.UserNotifications.Add(new UserNotification
            {
                NotificationId = entity.Id,
                UserId = ahmedId,
                IsRead = item.TryGetProperty("isRead", out var read) && read.GetBoolean()
            });
            await SaveBilingualAsync(db, EntityTypes.NotificationItem, entity.Id, item, ar.GetProperty("notifications")[index]);
            index++;
        }
    }

    private static async Task SeedDirectoryAsync(
        AppDbContext db,
        JsonElement en,
        JsonElement ar,
        IReadOnlyDictionary<string, int> branchIds,
        IReadOnlyDictionary<string, int> userIds)
    {
        var index = 0;
        foreach (var item in en.GetProperty("directory").EnumerateArray())
        {
            var branchKey = item.GetProperty("branchKey").GetString() ?? string.Empty;
            int? userId = null;
            if (item.TryGetProperty("userKey", out var userKey)
                && userKey.GetString() is { } userKeyValue
                && userIds.TryGetValue(userKeyValue, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var entity = new DirectoryMember
            {
                BranchId = branchIds[branchKey],
                Phone = item.GetProperty("phone").GetString() ?? string.Empty,
                Email = item.GetProperty("email").GetString() ?? string.Empty,
                UserId = userId
            };
            db.DirectoryMembers.Add(entity);
            await db.SaveChangesAsync();
            await SaveBilingualAsync(db, EntityTypes.DirectoryMember, entity.Id, item, ar.GetProperty("directory")[index]);
            index++;
        }
    }

    private static Task SeedContactSubmissionsAsync(AppDbContext db, JsonElement en)
    {
        if (!en.TryGetProperty("contactSubmissions", out var submissions))
        {
            return Task.CompletedTask;
        }

        foreach (var item in submissions.EnumerateArray())
        {
            db.ContactSubmissions.Add(new ContactSubmission
            {
                Name = item.GetProperty("name").GetString() ?? string.Empty,
                Email = item.GetProperty("email").GetString() ?? string.Empty,
                Subject = item.GetProperty("subject").GetString() ?? string.Empty,
                Message = item.GetProperty("message").GetString() ?? string.Empty,
                SubmittedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 14)),
                IsRead = item.TryGetProperty("isRead", out var read) && read.GetBoolean()
            });
        }

        return Task.CompletedTask;
    }

    private static Task SeedPasswordResetRequestsAsync(
        AppDbContext db,
        JsonElement en,
        IReadOnlyDictionary<string, int> userIds)
    {
        if (!en.TryGetProperty("passwordResetRequests", out var requests))
        {
            return Task.CompletedTask;
        }

        foreach (var item in requests.EnumerateArray())
        {
            var userKey = item.GetProperty("userKey").GetString() ?? string.Empty;
            if (!userIds.TryGetValue(userKey, out var userId))
            {
                continue;
            }

            db.PasswordResetRequests.Add(new PasswordResetRequest
            {
                UserId = userId,
                Email = item.GetProperty("email").GetString() ?? string.Empty,
                RequestedAt = DateTime.UtcNow.AddDays(-2),
                IsResolved = item.TryGetProperty("isResolved", out var resolved) && resolved.GetBoolean()
            });
        }

        return Task.CompletedTask;
    }

    private static async Task SaveBilingualAsync(AppDbContext db, string entityType, int entityId, JsonElement enItem, JsonElement arItem)
    {
        await TranslationStore.SaveAsync(db, entityType, entityId, "en", JsonSerializer.Deserialize<object>(enItem.GetRawText())!);
        await TranslationStore.SaveAsync(db, entityType, entityId, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
    }

    private static void SaveAppSetting(AppDbContext db, string key, JsonElement en, JsonElement ar, string property)
    {
        db.AppSettings.Add(new AppSetting
        {
            Key = key,
            ValueJson = JsonSerializer.Serialize(new Dictionary<string, JsonElement>
            {
                ["en"] = en.GetProperty(property),
                ["ar"] = ar.GetProperty(property)
            })
        });
    }

    private static DateTime ParseDate(string value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private static int ParseColorHex(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = value[2..];
        }

        if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static async Task<bool> IsSeedCompleteAsync(AppDbContext db) =>
        await db.FamilyBranches.AnyAsync();
}
