namespace BareqAlNaqool.Domain.Entities;

public class EntityTranslation
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Language { get; set; } = "en";
    public string DataJson { get; set; } = "{}";
}

public class LandingSlide
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
}

public class HomeStat
{
    public int Id { get; set; }
    public int TotalMembers { get; set; }
    public int FamilyBranches { get; set; }
    public int NewsUpdates { get; set; }
    public int UpcomingEvents { get; set; }
}

public class NewsItem
{
    public int Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public int CategoryColorHex { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public bool IsFeatured { get; set; }
    public string PublishStatus { get; set; } = "Published";
}

public class EventItem
{
    public int Id { get; set; }
    public DateTime EventDate { get; set; }
    public string TimeValue { get; set; } = string.Empty;
    public int? OrganizerUserId { get; set; }
    public string? CommitteeKey { get; set; }
    public bool IsPublic { get; set; } = true;
    public ICollection<EventRegistration> Registrations { get; set; } = [];
}

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public EventItem Event { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class FamilyBranch
{
    public int Id { get; set; }
    public int MemberCount { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public ICollection<BranchMember> Members { get; set; } = [];
}

public class BranchMember
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public FamilyBranch Branch { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
}

public class TreeMember
{
    public int Id { get; set; }
    public int Generation { get; set; }
    public bool IsFounder { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class Conversation
{
    public int Id { get; set; }
    public bool IsGroup { get; set; }
    public string TypeKey { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = [];
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
}

public class ConversationParticipant
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public int UserId { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public int? SenderUserId { get; set; }
    public string SenderNameKey { get; set; } = string.Empty;
    public bool IsAnnouncement { get; set; }
    public bool IsHidden { get; set; }
    public DateTime SentAt { get; set; }
}

public class Album
{
    public int Id { get; set; }
    public int PhotoCount { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public string GalleryTypeKey { get; set; } = "Albums";
}

public class GalleryPhoto
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
}

public class Document
{
    public int Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public string FileSize { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = "Public";
}

public class CouncilModule
{
    public int Id { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CouncilMeeting
{
    public int Id { get; set; }
    public DateTime MeetingDate { get; set; }
    public string TimeValue { get; set; } = string.Empty;
    public int Decisions { get; set; }
    public int Tasks { get; set; }
    public int Attachments { get; set; }
    public bool IsLatest { get; set; }
    public string? MinutesFileUrl { get; set; }
}

public class CouncilListItem
{
    public int Id { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string? StatusKey { get; set; }
}

public class DirectoryMember
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public FamilyBranch Branch { get; set; } = null!;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

public class NotificationItem
{
    public int Id { get; set; }
    public string TypeKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserNotification
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public NotificationItem Notification { get; set; } = null!;
    public int UserId { get; set; }
    public bool IsRead { get; set; }
}

public class ContactSubmission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

public class PasswordResetRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
}

public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string ValueJson { get; set; } = "[]";
}
