namespace BareqAlNaqool.Application.DTOs;

public record HomeStatsDto(int TotalMembers, int FamilyBranches, int NewsUpdates, int UpcomingEvents);

public record LandingSlideDto(string TitleLine1, string TitleLine2, string Subtitle);

public record NewsItemDto(
    string Id,
    string Category,
    string Title,
    string Description,
    string Body,
    string TimeAgo,
    string PublishedDate,
    int CategoryColorHex,
    string ImageUrl);

public record EventItemDto(
    string Id,
    string Day,
    string Month,
    string Title,
    string Location,
    string Time,
    string FullDate,
    string Description,
    string Organizer,
    bool IsMine);

public record FamilyBranchDto(string Id, string Name, int MemberCount, string Description, string ImageUrl);

public record BranchMemberDto(string Id, string Name, string Role, string BranchId, string ImageUrl);

public record TreeMemberDto(string Name, string Subtitle, int Generation, bool IsFounder, string ImageUrl);

public record ConversationDto(string Id, string Name, string LastMessage, string Time, int UnreadCount, bool IsGroup, string Type);

public record ChatMessageDto(string Id, string ConversationId, string SenderName, string Content, string Time, bool IsMe, bool IsAnnouncement);

public record SendMessageRequest(string Content);

public record DocumentItemDto(string Id, string Title, string FileSize, string Date, string Category, string Description);

public record AlbumItemDto(string Id, string Title, int PhotoCount, string ImageUrl, bool IsFeatured, string Description);

public record GalleryPhotoDto(string Id, string AlbumId, string Caption, string ImageUrl);

public record CouncilModuleDto(string Id, string IconName, string Label, string Subtitle);

public record MeetingInfoDto(string Title, string Date, string Time, string Location, int Decisions, int Tasks, int Attachments);

public record CouncilListItemDto(string Id, string Title, string Subtitle, string Meta, string? Status);

public record UserProfileDto(
    string Name,
    string Role,
    string MemberId,
    string Email,
    string Phone,
    string Branch,
    string DateOfBirth,
    string MaritalStatus,
    int ChildrenCount);

public record UpdateProfileRequest(
    string Name,
    string Role,
    string MemberId,
    string Email,
    string Phone,
    string Branch,
    string DateOfBirth,
    string MaritalStatus,
    int ChildrenCount);

public record AuthSessionDto(string Mode, string DisplayName, string? Email);

public record LoginRequest(string Username, string Password);

public record RegisterRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Username,
    string Password,
    string DateOfBirth,
    string Relation);

public record ForgotPasswordRequest(string Email);

public record AuthResponseDto(AuthSessionDto Session, string Token);

public record DirectoryMemberDto(
    string Id,
    string Name,
    string Role,
    string BranchId,
    string BranchName,
    string Phone,
    string Email,
    string City);

public record NotificationItemDto(string Id, string Title, string Body, string TimeAgo, string Type, bool IsRead);

public record ContactRequestDto(string Name, string Email, string Subject, string Message);

public record DashboardStatsDto(
    int TotalUsers,
    int TotalNews,
    int TotalEvents,
    int TotalBranches,
    int TotalAlbums,
    int TotalDocuments,
    int TotalNotifications,
    int TotalDirectoryMembers,
    int UnreadContacts);
