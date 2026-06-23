namespace BareqAlNaqool.Application.DTOs.Admin;

public record AdminNewsDto(
    string Id,
    string Category,
    string Title,
    string Description,
    string Body,
    string PublishedDate,
    int CategoryColorHex,
    string ImageUrl);

public record AdminNewsCreateDto(
    string Category,
    string Title,
    string Description,
    string Body,
    string PublishedDate,
    int CategoryColorHex,
    string ImageUrl);

public record AdminNewsUpdateDto(
    string Category,
    string Title,
    string Description,
    string Body,
    string PublishedDate,
    int CategoryColorHex,
    string ImageUrl);

public record AdminEventDto(
    string Id,
    string Day,
    string Month,
    string Title,
    string Location,
    string Time,
    string FullDate,
    string Description,
    string Organizer);

public record AdminEventCreateDto(
    string Day,
    string Month,
    string Title,
    string Location,
    string Time,
    string FullDate,
    string Description,
    string Organizer);

public record AdminEventUpdateDto(
    string Day,
    string Month,
    string Title,
    string Location,
    string Time,
    string FullDate,
    string Description,
    string Organizer);

public record AdminBranchDto(string Id, string Name, int MemberCount, string Description, string ImageUrl);

public record AdminBranchCreateDto(string Name, int MemberCount, string Description, string ImageUrl);

public record AdminBranchUpdateDto(string Name, int MemberCount, string Description, string ImageUrl);

public record AdminAlbumDto(string Id, string Title, int PhotoCount, string ImageUrl, bool IsFeatured, string Description);

public record AdminAlbumCreateDto(string Title, int PhotoCount, string ImageUrl, bool IsFeatured, string Description);

public record AdminAlbumUpdateDto(string Title, int PhotoCount, string ImageUrl, bool IsFeatured, string Description);

public record AdminDocumentDto(string Id, string Title, string FileSize, string Date, string Category, string Description);

public record AdminDocumentCreateDto(string Title, string FileSize, string Date, string Category, string Description);

public record AdminDocumentUpdateDto(string Title, string FileSize, string Date, string Category, string Description);

public record AdminNotificationDto(string Id, string Title, string Body, string Type);

public record AdminNotificationCreateDto(string Title, string Body, string Type);

public record AdminNotificationUpdateDto(string Title, string Body, string Type);

public record AdminCouncilItemDto(string Id, string ModuleId, string Title, string Subtitle, string Meta, string? Status);

public record AdminCouncilItemCreateDto(string ModuleId, string Title, string Subtitle, string Meta, string? Status);

public record AdminCouncilItemUpdateDto(string ModuleId, string Title, string Subtitle, string Meta, string? Status);

public record AdminDirectoryMemberDto(
    string Id,
    string Name,
    string Role,
    string BranchId,
    string BranchName,
    string Phone,
    string Email,
    string City);

public record AdminDirectoryMemberCreateDto(
    string Name,
    string Role,
    string BranchId,
    string Phone,
    string Email,
    string City);

public record AdminDirectoryMemberUpdateDto(
    string Name,
    string Role,
    string BranchId,
    string Phone,
    string Email,
    string City);

public record AdminUserDto(
    string Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string MemberId,
    string Branch,
    bool IsGuest);

public record AdminUserCreateDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role,
    string MemberId,
    string Branch);

public record AdminUserUpdateDto(
    string Email,
    string FullName,
    string Role,
    string MemberId,
    string Branch,
    string? Password);
