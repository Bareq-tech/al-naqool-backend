using BareqAlNaqool.Application.DTOs;

namespace BareqAlNaqool.Application.Interfaces;

public interface ILanguageContext
{
    string Language { get; }
}

public interface IHomeService
{
    Task<HomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default);
    Task<NewsItemDto> GetLatestNewsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LandingSlideDto>> GetLandingSlidesAsync(CancellationToken cancellationToken = default);
}

public interface INewsService
{
    Task<IReadOnlyList<NewsItemDto>> GetNewsAsync(string? category, CancellationToken cancellationToken = default);
    Task<NewsItemDto?> GetNewsByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

public interface IEventsService
{
    Task<IReadOnlyList<EventItemDto>> GetEventsAsync(string filter, int? userId, CancellationToken cancellationToken = default);
    Task<EventItemDto?> GetEventByIdAsync(string id, int? userId, CancellationToken cancellationToken = default);
    Task<bool> IsRegisteredAsync(string eventId, int userId, CancellationToken cancellationToken = default);
    Task RegisterForEventAsync(string eventId, int userId, CancellationToken cancellationToken = default);
}

public interface IFamilyTreeService
{
    Task<IReadOnlyList<FamilyBranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default);
    Task<FamilyBranchDto?> GetBranchByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchMemberDto>> GetBranchMembersAsync(string branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreeMemberDto>> GetFounderLineageAsync(CancellationToken cancellationToken = default);
}

public interface IMessagesService
{
    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string? type, int? userId, CancellationToken cancellationToken = default);
    Task<ConversationDto?> GetConversationByIdAsync(string id, int? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessageDto>> GetChatMessagesAsync(string conversationId, int? userId, CancellationToken cancellationToken = default);
    Task SendMessageAsync(string conversationId, string content, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFilterTypesAsync(CancellationToken cancellationToken = default);
}

public interface IGalleryService
{
    Task<IReadOnlyList<AlbumItemDto>> GetAlbumsAsync(string? type, CancellationToken cancellationToken = default);
    Task<AlbumItemDto?> GetAlbumByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GalleryPhotoDto>> GetAlbumPhotosAsync(string albumId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetGalleryTypesAsync(CancellationToken cancellationToken = default);
}

public interface IDocumentsService
{
    Task<IReadOnlyList<DocumentItemDto>> GetDocumentsAsync(string? category, CancellationToken cancellationToken = default);
    Task<DocumentItemDto?> GetDocumentByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

public interface ICouncilService
{
    Task<IReadOnlyList<CouncilModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default);
    Task<MeetingInfoDto> GetLatestMeetingAsync(CancellationToken cancellationToken = default);
    Task<string> GetPresidentNameAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CouncilListItemDto>> GetModuleItemsAsync(string moduleId, CancellationToken cancellationToken = default);
}

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> ContinueAsGuestAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthSessionDto?> GetSessionAsync(int? userId, CancellationToken cancellationToken = default);
    Task<TermsDto> GetTermsAsync(CancellationToken cancellationToken = default);
}

public interface IDirectoryService
{
    Task<IReadOnlyList<DirectoryMemberDto>> GetMembersAsync(string? query, string? branchId, CancellationToken cancellationToken = default);
    Task<DirectoryMemberDto?> GetMemberByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetBranchesAsync(CancellationToken cancellationToken = default);
}

public interface INotificationsService
{
    Task<IReadOnlyList<NotificationItemDto>> GetNotificationsAsync(int userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(string id, int userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IContactService
{
    Task SubmitContactAsync(ContactRequestDto request, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}

public interface IAdminCrudService<TDto, TCreateDto, TUpdateDto>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default);
    Task<TDto?> UpdateAsync(string id, TUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
