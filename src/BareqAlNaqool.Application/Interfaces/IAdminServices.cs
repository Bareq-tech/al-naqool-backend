using BareqAlNaqool.Application.DTOs.Admin;

namespace BareqAlNaqool.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResultDto> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string url, CancellationToken cancellationToken = default);
    string GetPublicPath(string storedPath);
}

public interface IAdminHomeService
{
    Task<AdminHomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default);
    Task<AdminHomeStatsDto> UpdateHomeStatsAsync(AdminHomeStatsUpdateDto dto, CancellationToken cancellationToken = default);
}

public interface IAdminPresidentService
{
    Task<AdminPresidentDto> GetAsync(CancellationToken cancellationToken = default);
    Task<AdminPresidentDto> UpdateAsync(AdminPresidentUpdateDto dto, CancellationToken cancellationToken = default);
}

public interface IAdminRegistrationService
{
    Task<IReadOnlyList<AdminRegistrationDto>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<bool> ApproveAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> RejectAsync(string userId, AdminRegistrationDecisionDto dto, CancellationToken cancellationToken = default);
}

public interface IAdminPasswordService
{
    Task<IReadOnlyList<AdminPasswordResetRequestDto>> GetPendingResetsAsync(CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string userId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResolveResetRequestAsync(string requestId, AdminResetPasswordDto dto, CancellationToken cancellationToken = default);
}

public interface IAdminRoleService
{
    Task<IReadOnlyList<AdminRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(string userId, AdminAssignRoleDto dto, CancellationToken cancellationToken = default);
}

public interface IAdminEventRegistrationService
{
    Task<IReadOnlyList<AdminEventRegistrationDto>> GetByEventAsync(string eventId, CancellationToken cancellationToken = default);
    Task<bool> CancelRegistrationAsync(string registrationId, CancellationToken cancellationToken = default);
}

public interface IAdminMessagingService
{
    Task<IReadOnlyList<AdminChatMessageDto>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default);
    Task<AdminChatMessageDto> BroadcastAsync(AdminBroadcastMessageDto dto, int adminUserId, CancellationToken cancellationToken = default);
    Task<bool> HideMessageAsync(string messageId, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAsync(string messageId, CancellationToken cancellationToken = default);
}

public interface IAdminContactService
{
    Task<IReadOnlyList<AdminContactSubmissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AdminContactSubmissionDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public interface IAdminSettingsService
{
    Task<AdminAppSettingDto?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
    Task<AdminAppSettingDto> UpdateSettingAsync(string key, AdminAppSettingUpdateDto dto, CancellationToken cancellationToken = default);
    Task<AdminLocalizedListSettingDto> GetLocalizedListAsync(string key, CancellationToken cancellationToken = default);
    Task<AdminLocalizedListSettingDto> UpdateLocalizedListAsync(string key, AdminLocalizedListSettingUpdateDto dto, CancellationToken cancellationToken = default);
    Task<AdminTermsDto> GetTermsAsync(CancellationToken cancellationToken = default);
    Task<AdminTermsDto> UpdateTermsAsync(AdminTermsUpdateDto dto, CancellationToken cancellationToken = default);
    Task<AdminGuestPermissionsDto> GetGuestPermissionsAsync(CancellationToken cancellationToken = default);
    Task<AdminGuestPermissionsDto> UpdateGuestPermissionsAsync(AdminGuestPermissionsUpdateDto dto, CancellationToken cancellationToken = default);
    Task<AdminBrandingDto> GetBrandingAsync(CancellationToken cancellationToken = default);
    Task<AdminBrandingDto> UpdateBrandingAsync(AdminBrandingUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminQuickAccessTileDto>> GetQuickAccessTilesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminQuickAccessTileDto>> UpdateQuickAccessTilesAsync(AdminQuickAccessTilesUpdateDto dto, CancellationToken cancellationToken = default);
}
