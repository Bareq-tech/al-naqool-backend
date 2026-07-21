namespace BareqAlNaqool.Application.DTOs.Admin;

public record AdminNewsDto(
    string Id,
    string CategoryEn,
    string CategoryAr,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    string BodyEn,
    string BodyAr,
    string PublishedDateEn,
    string PublishedDateAr,
    int CategoryColorHex,
    string ImageUrl,
    bool IsFeatured,
    string PublishStatus,
    DateTime PublishedAt);

public record AdminNewsCreateDto(
    string CategoryEn,
    string CategoryAr,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    string BodyEn,
    string BodyAr,
    string PublishedDateEn,
    string PublishedDateAr,
    int CategoryColorHex,
    string ImageUrl,
    bool IsFeatured,
    string PublishStatus,
    DateTime? PublishedAt);

public record AdminNewsUpdateDto(
    string CategoryEn,
    string CategoryAr,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    string BodyEn,
    string BodyAr,
    string PublishedDateEn,
    string PublishedDateAr,
    int CategoryColorHex,
    string ImageUrl,
    bool IsFeatured,
    string PublishStatus,
    DateTime? PublishedAt);

public record AdminEventDto(
    string Id,
    string DayEn,
    string DayAr,
    string MonthEn,
    string MonthAr,
    string TitleEn,
    string TitleAr,
    string LocationEn,
    string LocationAr,
    string Time,
    string FullDateEn,
    string FullDateAr,
    string DescriptionEn,
    string DescriptionAr,
    string OrganizerEn,
    string OrganizerAr,
    bool IsPublic,
    string? OrganizerUserId,
    string? CommitteeKey,
    DateTime EventDate);

public record AdminEventCreateDto(
    string DayEn,
    string DayAr,
    string MonthEn,
    string MonthAr,
    string TitleEn,
    string TitleAr,
    string LocationEn,
    string LocationAr,
    string Time,
    string FullDateEn,
    string FullDateAr,
    string DescriptionEn,
    string DescriptionAr,
    string OrganizerEn,
    string OrganizerAr,
    bool IsPublic,
    string? OrganizerUserId,
    string? CommitteeKey,
    DateTime EventDate);

public record AdminEventUpdateDto(
    string DayEn,
    string DayAr,
    string MonthEn,
    string MonthAr,
    string TitleEn,
    string TitleAr,
    string LocationEn,
    string LocationAr,
    string Time,
    string FullDateEn,
    string FullDateAr,
    string DescriptionEn,
    string DescriptionAr,
    string OrganizerEn,
    string OrganizerAr,
    bool IsPublic,
    string? OrganizerUserId,
    string? CommitteeKey,
    DateTime EventDate);

public record AdminEventRegistrationDto(
    string Id,
    string EventId,
    string EventTitle,
    string UserId,
    string UserName,
    string UserEmail,
    DateTime RegisteredAt);

public record AdminBranchDto(
    string Id,
    string NameEn,
    string NameAr,
    int MemberCount,
    string DescriptionEn,
    string DescriptionAr,
    string ImageUrl);

public record AdminBranchCreateDto(
    string NameEn,
    string NameAr,
    int MemberCount,
    string DescriptionEn,
    string DescriptionAr,
    string ImageUrl);

public record AdminBranchUpdateDto(
    string NameEn,
    string NameAr,
    int MemberCount,
    string DescriptionEn,
    string DescriptionAr,
    string ImageUrl);

public record AdminBranchMemberDto(
    string Id,
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string ImageUrl);

public record AdminBranchMemberCreateDto(
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string ImageUrl);

public record AdminBranchMemberUpdateDto(
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string ImageUrl);

public record AdminTreeMemberDto(
    string Id,
    string NameEn,
    string NameAr,
    string SubtitleEn,
    string SubtitleAr,
    string? ParentId,
    string? SpouseId,
    int Generation,
    bool IsFounder,
    string ImageUrl,
    int SortOrder);

public record AdminTreeMemberCreateDto(
    string NameEn,
    string NameAr,
    string SubtitleEn,
    string SubtitleAr,
    string? ParentId,
    string? SpouseId,
    string ImageUrl);

public record AdminTreeMemberUpdateDto(
    string NameEn,
    string NameAr,
    string SubtitleEn,
    string SubtitleAr,
    string? ParentId,
    string? SpouseId,
    string ImageUrl);

public record AdminAlbumDto(
    string Id,
    string TitleEn,
    string TitleAr,
    int PhotoCount,
    string ImageUrl,
    bool IsFeatured,
    string DescriptionEn,
    string DescriptionAr,
    string GalleryTypeKey);

public record AdminAlbumCreateDto(
    string TitleEn,
    string TitleAr,
    int PhotoCount,
    string ImageUrl,
    bool IsFeatured,
    string DescriptionEn,
    string DescriptionAr,
    string GalleryTypeKey);

public record AdminAlbumUpdateDto(
    string TitleEn,
    string TitleAr,
    int PhotoCount,
    string ImageUrl,
    bool IsFeatured,
    string DescriptionEn,
    string DescriptionAr,
    string GalleryTypeKey);

public record AdminGalleryPhotoDto(
    string Id,
    string AlbumId,
    string CaptionEn,
    string CaptionAr,
    string ImageUrl);

public record AdminGalleryPhotoCreateDto(
    string AlbumId,
    string CaptionEn,
    string CaptionAr,
    string ImageUrl);

public record AdminGalleryPhotoUpdateDto(
    string CaptionEn,
    string CaptionAr,
    string ImageUrl);

public record AdminDocumentDto(
    string Id,
    string TitleEn,
    string TitleAr,
    string FileSize,
    string DateEn,
    string DateAr,
    string Category,
    string DescriptionEn,
    string DescriptionAr,
    string FileUrl,
    string AccessLevel);

public record AdminDocumentCreateDto(
    string TitleEn,
    string TitleAr,
    string FileSize,
    string DateEn,
    string DateAr,
    string Category,
    string DescriptionEn,
    string DescriptionAr,
    string FileUrl,
    string AccessLevel);

public record AdminDocumentUpdateDto(
    string TitleEn,
    string TitleAr,
    string FileSize,
    string DateEn,
    string DateAr,
    string Category,
    string DescriptionEn,
    string DescriptionAr,
    string FileUrl,
    string AccessLevel);

public record AdminNotificationDto(
    string Id,
    string TitleEn,
    string TitleAr,
    string BodyEn,
    string BodyAr,
    string Type);

public record AdminNotificationCreateDto(
    string TitleEn,
    string TitleAr,
    string BodyEn,
    string BodyAr,
    string Type);

public record AdminNotificationUpdateDto(
    string TitleEn,
    string TitleAr,
    string BodyEn,
    string BodyAr,
    string Type);

public record AdminCouncilItemDto(
    string Id,
    string ModuleId,
    string TitleEn,
    string TitleAr,
    string SubtitleEn,
    string SubtitleAr,
    string MetaEn,
    string MetaAr,
    string? Status);

public record AdminCouncilItemCreateDto(
    string ModuleId,
    string TitleEn,
    string TitleAr,
    string SubtitleEn,
    string SubtitleAr,
    string MetaEn,
    string MetaAr,
    string? Status);

public record AdminCouncilItemUpdateDto(
    string ModuleId,
    string TitleEn,
    string TitleAr,
    string SubtitleEn,
    string SubtitleAr,
    string MetaEn,
    string MetaAr,
    string? Status);

public record AdminCouncilModuleDto(
    string Id,
    string ModuleKey,
    string IconName,
    string LabelEn,
    string LabelAr,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminCouncilModuleCreateDto(
    string ModuleKey,
    string IconName,
    string LabelEn,
    string LabelAr,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminCouncilModuleUpdateDto(
    string ModuleKey,
    string IconName,
    string LabelEn,
    string LabelAr,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminCouncilMeetingDto(
    string Id,
    string TitleEn,
    string TitleAr,
    string DateEn,
    string DateAr,
    string Time,
    string LocationEn,
    string LocationAr,
    int Decisions,
    int Tasks,
    int Attachments,
    bool IsLatest,
    string? MinutesFileUrl);

public record AdminCouncilMeetingCreateDto(
    string TitleEn,
    string TitleAr,
    string DateEn,
    string DateAr,
    string Time,
    string LocationEn,
    string LocationAr,
    int Decisions,
    int Tasks,
    int Attachments,
    bool IsLatest,
    string? MinutesFileUrl,
    DateTime MeetingDate);

public record AdminCouncilMeetingUpdateDto(
    string TitleEn,
    string TitleAr,
    string DateEn,
    string DateAr,
    string Time,
    string LocationEn,
    string LocationAr,
    int Decisions,
    int Tasks,
    int Attachments,
    bool IsLatest,
    string? MinutesFileUrl,
    DateTime MeetingDate);

public record AdminPresidentDto(string NameEn, string NameAr);

public record AdminPresidentUpdateDto(string NameEn, string NameAr);

public record AdminDirectoryMemberDto(
    string Id,
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string BranchNameEn,
    string BranchNameAr,
    string Phone,
    string Email,
    string CityEn,
    string CityAr);

public record AdminDirectoryMemberCreateDto(
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string Phone,
    string Email,
    string CityEn,
    string CityAr);

public record AdminDirectoryMemberUpdateDto(
    string NameEn,
    string NameAr,
    string RoleEn,
    string RoleAr,
    string BranchId,
    string Phone,
    string Email,
    string CityEn,
    string CityAr);

public record AdminUserDto(
    string Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string MemberId,
    string Branch,
    string Phone,
    string DateOfBirth,
    string MaritalStatus,
    int ChildrenCount,
    string AccountStatus,
    string? RegistrationRelation,
    bool IsGuest,
    DateTime? TermsAcceptedAt);

public record AdminUserCreateDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role,
    string MemberId,
    string Branch,
    string Phone,
    string DateOfBirth,
    string MaritalStatus,
    int ChildrenCount,
    string AccountStatus);

public record AdminUserUpdateDto(
    string Email,
    string FullName,
    string Role,
    string MemberId,
    string Branch,
    string Phone,
    string DateOfBirth,
    string MaritalStatus,
    int ChildrenCount,
    string AccountStatus,
    string? Password);

public record AdminLandingSlideDto(
    string Id,
    string TitleLine1En,
    string TitleLine1Ar,
    string TitleLine2En,
    string TitleLine2Ar,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminLandingSlideCreateDto(
    string TitleLine1En,
    string TitleLine1Ar,
    string TitleLine2En,
    string TitleLine2Ar,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminLandingSlideUpdateDto(
    string TitleLine1En,
    string TitleLine1Ar,
    string TitleLine2En,
    string TitleLine2Ar,
    string SubtitleEn,
    string SubtitleAr,
    int SortOrder);

public record AdminHomeStatsDto(
    int TotalMembers,
    int FamilyBranches,
    int NewsUpdates,
    int UpcomingEvents);

public record AdminHomeStatsUpdateDto(
    int TotalMembers,
    int FamilyBranches,
    int NewsUpdates,
    int UpcomingEvents);

public record AdminConversationDto(
    string Id,
    string NameEn,
    string NameAr,
    string LastMessageEn,
    string LastMessageAr,
    bool IsGroup,
    string Type,
    DateTime LastMessageAt,
    IReadOnlyList<string> ParticipantUserIds);

public record AdminConversationCreateDto(
    string NameEn,
    string NameAr,
    bool IsGroup,
    string Type,
    IReadOnlyList<string> ParticipantUserIds);

public record AdminConversationUpdateDto(
    string NameEn,
    string NameAr,
    bool IsGroup,
    string Type,
    IReadOnlyList<string> ParticipantUserIds);

public record AdminChatMessageDto(
    string Id,
    string ConversationId,
    string SenderNameEn,
    string SenderNameAr,
    string ContentEn,
    string ContentAr,
    bool IsAnnouncement,
    bool IsHidden,
    DateTime SentAt,
    string? SenderUserId);

public record AdminBroadcastMessageDto(
    string ConversationId,
    string ContentEn,
    string ContentAr,
    bool IsAnnouncement);

public record AdminContactSubmissionDto(
    string Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    DateTime SubmittedAt,
    bool IsRead);

public record AdminAppSettingDto(string Key, string ValueJson);

public record AdminAppSettingUpdateDto(string ValueJson);

public record AdminLocalizedListSettingDto(string Key, IReadOnlyList<string> En, IReadOnlyList<string> Ar);

public record AdminLocalizedListSettingUpdateDto(IReadOnlyList<string> En, IReadOnlyList<string> Ar);

public record AdminTermsDto(string En, string Ar);

public record AdminTermsUpdateDto(string En, string Ar);

public record AdminGuestPermissionsDto(
    bool News,
    bool Events,
    bool Gallery,
    bool Documents,
    bool Council,
    bool Messages,
    bool Directory,
    bool FamilyTree);

public record AdminGuestPermissionsUpdateDto(
    bool News,
    bool Events,
    bool Gallery,
    bool Documents,
    bool Council,
    bool Messages,
    bool Directory,
    bool FamilyTree);

public record AdminBrandingDto(
    string AppNameEn,
    string AppNameAr,
    string LogoUrl,
    string PrimaryColorHex);

public record AdminBrandingUpdateDto(
    string AppNameEn,
    string AppNameAr,
    string LogoUrl,
    string PrimaryColorHex);

public record AdminQuickAccessTileDto(string Key, string LabelEn, string LabelAr, string IconName, string Route, int SortOrder);

public record AdminQuickAccessTilesUpdateDto(IReadOnlyList<AdminQuickAccessTileDto> Tiles);

public record AdminRegistrationDto(
    string Id,
    string Username,
    string Email,
    string FullName,
    string Phone,
    string DateOfBirth,
    string Relation,
    DateTime CreatedAt);

public record AdminRegistrationDecisionDto(string? Reason);

public record AdminPasswordResetRequestDto(
    string Id,
    string UserId,
    string Email,
    DateTime RequestedAt,
    bool IsResolved);

public record AdminResetPasswordDto(string NewPassword);

public record AdminRoleDto(string Name);

public record FileUploadResultDto(string Url, string FileName, long SizeBytes);

public record AdminAssignRoleDto(string Role);
