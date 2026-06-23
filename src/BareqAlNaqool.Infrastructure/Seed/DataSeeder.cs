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

        if (await db.HomeStats.AnyAsync())
        {
            return;
        }

        var seedPath = Path.Combine(environment.ContentRootPath, "Seed");
        if (!Directory.Exists(seedPath))
        {
            seedPath = Path.Combine(AppContext.BaseDirectory, "Seed");
        }

        var enJson = await File.ReadAllTextAsync(Path.Combine(seedPath, "en.json"));
        var arJson = await File.ReadAllTextAsync(Path.Combine(seedPath, "ar.json"));
        var en = JsonDocument.Parse(enJson).RootElement;
        var ar = JsonDocument.Parse(arJson).RootElement;

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var ahmed = new ApplicationUser
        {
            UserName = "ahmed",
            Email = "ahmed.alnaqool@email.com",
            FullName = "Ahmed Abdullah Al Naqool",
            DisplayRole = "Family Member",
            MemberId = "AN-2024-0847",
            PhoneNumber = "+966 50 123 4567",
            Branch = "Al Mofreh",
            DateOfBirth = "March 15, 1985",
            MaritalStatus = "Married",
            ChildrenCount = 3,
            EmailConfirmed = true,
            AccountStatus = AccountStatuses.Active
        };
        await userManager.CreateAsync(ahmed, "password123");
        await userManager.AddToRoleAsync(ahmed, AppRoles.Member);

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@alnaqool.com",
            FullName = "System Administrator",
            DisplayRole = "Administrator",
            MemberId = "ADMIN-001",
            Branch = "Al Naqool",
            EmailConfirmed = true,
            AccountStatus = AccountStatuses.Active
        };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, AppRoles.Admin);

        await TranslationStore.SaveAsync(db, EntityTypes.President, 1, "en", new { name = en.GetProperty("president").GetString() });
        await TranslationStore.SaveAsync(db, EntityTypes.President, 1, "ar", new { name = ar.GetProperty("president").GetString() });

        var homeStats = en.GetProperty("homeStats");
        db.HomeStats.Add(new HomeStat
        {
            TotalMembers = homeStats.GetProperty("totalMembers").GetInt32(),
            FamilyBranches = homeStats.GetProperty("familyBranches").GetInt32(),
            NewsUpdates = homeStats.GetProperty("newsUpdates").GetInt32(),
            UpcomingEvents = homeStats.GetProperty("upcomingEvents").GetInt32()
        });

        var slideIndex = 0;
        foreach (var slide in en.GetProperty("landingSlides").EnumerateArray())
        {
            slideIndex++;
            var entity = new LandingSlide { SortOrder = slideIndex };
            db.LandingSlides.Add(entity);
            await db.SaveChangesAsync();
            var arSlide = ar.GetProperty("landingSlides")[slideIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.LandingSlide, entity.Id, "en", JsonSerializer.Deserialize<object>(slide.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.LandingSlide, entity.Id, "ar", JsonSerializer.Deserialize<object>(arSlide.GetRawText())!);
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
                ["en"] = "By registering you agree to the family app terms and privacy policy.",
                ["ar"] = "بالتسجيل فإنك توافق على شروط تطبيق العائلة وسياسة الخصوصية."
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
                new { key = "gallery", labelEn = "Gallery", labelAr = "المعرض", iconName = "photo", route = "/gallery", sortOrder = 3 }
            })
        });

        var newsIndex = 0;
        foreach (var item in en.GetProperty("newsItems").EnumerateArray())
        {
            newsIndex++;
            var entity = new NewsItem
            {
                CategoryKey = item.GetProperty("category").GetString() ?? string.Empty,
                CategoryColorHex = ParseColorHex(item.GetProperty("categoryColorHex").GetString() ?? "0"),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                PublishedAt = ParseDate(item.GetProperty("publishedDate").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
            };
            db.NewsItems.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("newsItems")[newsIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.NewsItem, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.NewsItem, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var eventIndex = 0;
        var registeredEventIds = new List<int>();
        foreach (var item in en.GetProperty("events").EnumerateArray())
        {
            eventIndex++;
            var entity = new EventItem
            {
                EventDate = ParseDate(item.GetProperty("fullDate").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                TimeValue = item.GetProperty("time").GetString() ?? string.Empty
            };
            db.Events.Add(entity);
            await db.SaveChangesAsync();
            if (item.TryGetProperty("isMine", out var isMine) && isMine.GetBoolean())
            {
                registeredEventIds.Add(entity.Id);
            }

            var arItem = ar.GetProperty("events")[eventIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.EventItem, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.EventItem, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        foreach (var eventId in registeredEventIds)
        {
            db.EventRegistrations.Add(new EventRegistration
            {
                EventId = eventId,
                UserId = ahmed.Id,
                RegisteredAt = DateTime.UtcNow
            });
        }

        var branchIndex = 0;
        foreach (var item in en.GetProperty("branches").EnumerateArray())
        {
            branchIndex++;
            var entity = new FamilyBranch
            {
                MemberCount = item.GetProperty("memberCount").GetInt32(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.FamilyBranches.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("branches")[branchIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.FamilyBranch, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.FamilyBranch, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var memberIndex = 0;
        foreach (var item in en.GetProperty("branchMembers").EnumerateArray())
        {
            memberIndex++;
            var entity = new BranchMember
            {
                BranchId = int.Parse(item.GetProperty("branchId").GetString() ?? "1", CultureInfo.InvariantCulture),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.BranchMembers.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("branchMembers")[memberIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.BranchMember, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.BranchMember, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var treeIndex = 0;
        foreach (var item in en.GetProperty("founderLineage").EnumerateArray())
        {
            treeIndex++;
            var entity = new TreeMember
            {
                Generation = item.GetProperty("generation").GetInt32(),
                IsFounder = item.TryGetProperty("isFounder", out var founder) && founder.GetBoolean(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                SortOrder = treeIndex
            };
            db.TreeMembers.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("founderLineage")[treeIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.TreeMember, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.TreeMember, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var conversationIndex = 0;
        foreach (var item in en.GetProperty("conversations").EnumerateArray())
        {
            conversationIndex++;
            var entity = new Conversation
            {
                IsGroup = item.GetProperty("isGroup").GetBoolean(),
                TypeKey = item.GetProperty("type").GetString() ?? string.Empty,
                LastMessageAt = DateTime.UtcNow.AddHours(-conversationIndex)
            };
            db.Conversations.Add(entity);
            await db.SaveChangesAsync();
            db.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = entity.Id,
                UserId = ahmed.Id,
                UnreadCount = item.GetProperty("unreadCount").GetInt32()
            });
            var arItem = ar.GetProperty("conversations")[conversationIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.Conversation, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.Conversation, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var messageIndex = 0;
        foreach (var item in en.GetProperty("chatMessages").EnumerateArray())
        {
            messageIndex++;
            var isMe = item.TryGetProperty("isMe", out var me) && me.GetBoolean();
            var entity = new ChatMessage
            {
                ConversationId = int.Parse(item.GetProperty("conversationId").GetString() ?? "1", CultureInfo.InvariantCulture),
                SenderUserId = isMe ? ahmed.Id : null,
                SenderNameKey = item.GetProperty("senderName").GetString() ?? string.Empty,
                IsAnnouncement = item.TryGetProperty("isAnnouncement", out var ann) && ann.GetBoolean(),
                SentAt = DateTime.UtcNow.AddHours(-messageIndex)
            };
            db.ChatMessages.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("chatMessages")[messageIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var albumIndex = 0;
        foreach (var item in en.GetProperty("albums").EnumerateArray())
        {
            albumIndex++;
            var entity = new Album
            {
                PhotoCount = item.GetProperty("photoCount").GetInt32(),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty,
                IsFeatured = item.GetProperty("isFeatured").GetBoolean(),
                GalleryTypeKey = "Albums"
            };
            db.Albums.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("albums")[albumIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.Album, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.Album, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var photoIndex = 0;
        foreach (var item in en.GetProperty("galleryPhotos").EnumerateArray())
        {
            photoIndex++;
            var entity = new GalleryPhoto
            {
                AlbumId = int.Parse(item.GetProperty("albumId").GetString() ?? "1", CultureInfo.InvariantCulture),
                ImageUrl = item.GetProperty("imageUrl").GetString() ?? string.Empty
            };
            db.GalleryPhotos.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("galleryPhotos")[photoIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.GalleryPhoto, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.GalleryPhoto, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var docIndex = 0;
        foreach (var item in en.GetProperty("documents").EnumerateArray())
        {
            docIndex++;
            var entity = new Document
            {
                CategoryKey = item.GetProperty("category").GetString() ?? string.Empty,
                DocumentDate = ParseDate(item.GetProperty("date").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                FileSize = item.GetProperty("fileSize").GetString() ?? string.Empty
            };
            db.Documents.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("documents")[docIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.Document, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.Document, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var moduleIndex = 0;
        foreach (var item in en.GetProperty("councilModules").EnumerateArray())
        {
            moduleIndex++;
            var entity = new CouncilModule
            {
                ModuleKey = item.GetProperty("id").GetString() ?? string.Empty,
                IconName = item.GetProperty("iconName").GetString() ?? string.Empty,
                SortOrder = moduleIndex
            };
            db.CouncilModules.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("councilModules")[moduleIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.CouncilModule, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.CouncilModule, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var meeting = en.GetProperty("latestMeeting");
        var meetingEntity = new CouncilMeeting
        {
            MeetingDate = ParseDate(meeting.GetProperty("date").GetString() ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            TimeValue = meeting.GetProperty("time").GetString() ?? string.Empty,
            Decisions = meeting.GetProperty("decisions").GetInt32(),
            Tasks = meeting.GetProperty("tasks").GetInt32(),
            Attachments = meeting.GetProperty("attachments").GetInt32(),
            IsLatest = true
        };
        db.CouncilMeetings.Add(meetingEntity);
        await db.SaveChangesAsync();
        var arMeeting = ar.GetProperty("latestMeeting");
        await TranslationStore.SaveAsync(db, EntityTypes.CouncilMeeting, meetingEntity.Id, "en", JsonSerializer.Deserialize<object>(meeting.GetRawText())!);
        await TranslationStore.SaveAsync(db, EntityTypes.CouncilMeeting, meetingEntity.Id, "ar", JsonSerializer.Deserialize<object>(arMeeting.GetRawText())!);

        foreach (var moduleKey in new[] { "meetings", "committees", "voting", "tasks", "decisions", "members" })
        {
            var listIndex = 0;
            foreach (var item in en.GetProperty("councilItems").GetProperty(moduleKey).EnumerateArray())
            {
                listIndex++;
                var entity = new CouncilListItem
                {
                    ModuleKey = moduleKey,
                    StatusKey = item.TryGetProperty("status", out var status) ? status.GetString() : null
                };
                db.CouncilListItems.Add(entity);
                await db.SaveChangesAsync();
                var arItem = ar.GetProperty("councilItems").GetProperty(moduleKey)[listIndex - 1];
                await TranslationStore.SaveAsync(db, EntityTypes.CouncilListItem, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
                await TranslationStore.SaveAsync(db, EntityTypes.CouncilListItem, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
            }
        }

        var notifIndex = 0;
        foreach (var item in en.GetProperty("notifications").EnumerateArray())
        {
            notifIndex++;
            var entity = new NotificationItem
            {
                TypeKey = item.GetProperty("type").GetString() ?? string.Empty,
                CreatedAt = DateTime.UtcNow.AddHours(-notifIndex * 5)
            };
            db.Notifications.Add(entity);
            await db.SaveChangesAsync();
            db.UserNotifications.Add(new UserNotification
            {
                NotificationId = entity.Id,
                UserId = ahmed.Id,
                IsRead = item.TryGetProperty("isRead", out var read) && read.GetBoolean()
            });
            var arItem = ar.GetProperty("notifications")[notifIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.NotificationItem, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.NotificationItem, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        var dirIndex = 0;
        foreach (var item in en.GetProperty("directory").EnumerateArray())
        {
            dirIndex++;
            var email = item.GetProperty("email").GetString() ?? string.Empty;
            var entity = new DirectoryMember
            {
                BranchId = int.Parse(item.GetProperty("branchId").GetString() ?? "1", CultureInfo.InvariantCulture),
                Phone = item.GetProperty("phone").GetString() ?? string.Empty,
                Email = email,
                UserId = string.Equals(email, ahmed.Email, StringComparison.OrdinalIgnoreCase) ? ahmed.Id : null
            };
            db.DirectoryMembers.Add(entity);
            await db.SaveChangesAsync();
            var arItem = ar.GetProperty("directory")[dirIndex - 1];
            await TranslationStore.SaveAsync(db, EntityTypes.DirectoryMember, entity.Id, "en", JsonSerializer.Deserialize<object>(item.GetRawText())!);
            await TranslationStore.SaveAsync(db, EntityTypes.DirectoryMember, entity.Id, "ar", JsonSerializer.Deserialize<object>(arItem.GetRawText())!);
        }

        await db.SaveChangesAsync();
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
}
