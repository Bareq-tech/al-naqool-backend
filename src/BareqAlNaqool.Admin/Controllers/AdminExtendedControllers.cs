using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Admin.Controllers;

[Route("api/admin/branch-members")]
[Authorize(Policy = "AdminOnly")]
[ApiController]
public class BranchMembersAdminController(IAdminCrudService<AdminBranchMemberDto, AdminBranchMemberCreateDto, AdminBranchMemberUpdateDto> service)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminBranchMemberCreateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CreateAsync(dto, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AdminBranchMemberUpdateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var item = await service.UpdateAsync(id, dto, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[Route("api/admin/tree-members")]
[Authorize(Policy = "AdminOnly")]
[ApiController]
public class TreeMembersAdminController(IAdminCrudService<AdminTreeMemberDto, AdminTreeMemberCreateDto, AdminTreeMemberUpdateDto> service)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminTreeMemberCreateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CreateAsync(dto, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var item = await service.UpdateAsync(id, dto, cancellationToken);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[Route("api/admin/landing-slides")]
public class LandingSlidesAdminController(IAdminCrudService<AdminLandingSlideDto, AdminLandingSlideCreateDto, AdminLandingSlideUpdateDto> service)
    : AdminCrudController<AdminLandingSlideDto, AdminLandingSlideCreateDto, AdminLandingSlideUpdateDto>(service);

[Route("api/admin/gallery-photos")]
public class GalleryPhotosAdminController(IAdminCrudService<AdminGalleryPhotoDto, AdminGalleryPhotoCreateDto, AdminGalleryPhotoUpdateDto> service)
    : AdminCrudController<AdminGalleryPhotoDto, AdminGalleryPhotoCreateDto, AdminGalleryPhotoUpdateDto>(service);

[Route("api/admin/council-modules")]
public class CouncilModulesAdminController(IAdminCrudService<AdminCouncilModuleDto, AdminCouncilModuleCreateDto, AdminCouncilModuleUpdateDto> service)
    : AdminCrudController<AdminCouncilModuleDto, AdminCouncilModuleCreateDto, AdminCouncilModuleUpdateDto>(service);

[Route("api/admin/council-meetings")]
public class CouncilMeetingsAdminController(IAdminCrudService<AdminCouncilMeetingDto, AdminCouncilMeetingCreateDto, AdminCouncilMeetingUpdateDto> service)
    : AdminCrudController<AdminCouncilMeetingDto, AdminCouncilMeetingCreateDto, AdminCouncilMeetingUpdateDto>(service);

[Route("api/admin/conversations")]
public class ConversationsAdminController(IAdminCrudService<AdminConversationDto, AdminConversationCreateDto, AdminConversationUpdateDto> service)
    : AdminCrudController<AdminConversationDto, AdminConversationCreateDto, AdminConversationUpdateDto>(service);

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/home-stats")]
public class HomeStatsAdminController(IAdminHomeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await service.GetHomeStatsAsync(cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AdminHomeStatsUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateHomeStatsAsync(dto, cancellationToken));
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/president")]
public class PresidentAdminController(IAdminPresidentService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await service.GetAsync(cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AdminPresidentUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateAsync(dto, cancellationToken));
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/registrations")]
public class RegistrationsAdminController(IAdminRegistrationService service) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
        => Ok(await service.GetPendingAsync(cancellationToken));

    [HttpPost("{userId}/approve")]
    public async Task<IActionResult> Approve(string userId, CancellationToken cancellationToken)
        => await service.ApproveAsync(userId, cancellationToken) ? Ok() : NotFound();

    [HttpPost("{userId}/reject")]
    public async Task<IActionResult> Reject(string userId, [FromBody] AdminRegistrationDecisionDto dto, CancellationToken cancellationToken)
        => await service.RejectAsync(userId, dto, cancellationToken) ? Ok() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/password-resets")]
public class PasswordResetsAdminController(IAdminPasswordService service) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
        => Ok(await service.GetPendingResetsAsync(cancellationToken));

    [HttpPost("users/{userId}/reset")]
    public async Task<IActionResult> ResetUserPassword(string userId, [FromBody] AdminResetPasswordDto dto, CancellationToken cancellationToken)
        => await service.ResetPasswordAsync(userId, dto, cancellationToken) ? Ok() : NotFound();

    [HttpPost("{requestId}/resolve")]
    public async Task<IActionResult> Resolve(string requestId, [FromBody] AdminResetPasswordDto dto, CancellationToken cancellationToken)
        => await service.ResolveResetRequestAsync(requestId, dto, cancellationToken) ? Ok() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/roles")]
public class RolesAdminController(IAdminRoleService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetRolesAsync(cancellationToken));

    [HttpPut("users/{userId}")]
    public async Task<IActionResult> Assign(string userId, [FromBody] AdminAssignRoleDto dto, CancellationToken cancellationToken)
        => await service.AssignRoleAsync(userId, dto, cancellationToken) ? Ok() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/event-registrations")]
public class EventRegistrationsAdminController(IAdminEventRegistrationService service) : ControllerBase
{
    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetByEvent(string eventId, CancellationToken cancellationToken)
        => Ok(await service.GetByEventAsync(eventId, cancellationToken));

    [HttpDelete("{registrationId}")]
    public async Task<IActionResult> Cancel(string registrationId, CancellationToken cancellationToken)
        => await service.CancelRegistrationAsync(registrationId, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/messages")]
public class MessagesAdminController(IAdminMessagingService service) : ControllerBase
{
    [HttpGet("conversations/{conversationId}")]
    public async Task<IActionResult> GetMessages(string conversationId, CancellationToken cancellationToken)
        => Ok(await service.GetMessagesAsync(conversationId, cancellationToken));

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] AdminBroadcastMessageDto dto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        return Ok(await service.BroadcastAsync(dto, userId, cancellationToken));
    }

    [HttpPost("{messageId}/hide")]
    public async Task<IActionResult> Hide(string messageId, CancellationToken cancellationToken)
        => await service.HideMessageAsync(messageId, cancellationToken) ? Ok() : NotFound();

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> Delete(string messageId, CancellationToken cancellationToken)
        => await service.DeleteMessageAsync(messageId, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/contacts")]
public class ContactsAdminController(IAdminContactService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(string id, CancellationToken cancellationToken)
        => await service.MarkReadAsync(id, cancellationToken) ? Ok() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/settings")]
public class SettingsAdminController(IAdminSettingsService service) : ControllerBase
{
    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken cancellationToken)
    {
        var item = await service.GetSettingAsync(key, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] AdminAppSettingUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateSettingAsync(key, dto, cancellationToken));

    [HttpGet("lists/{key}")]
    public async Task<IActionResult> GetList(string key, CancellationToken cancellationToken)
        => Ok(await service.GetLocalizedListAsync(key, cancellationToken));

    [HttpPut("lists/{key}")]
    public async Task<IActionResult> UpdateList(string key, [FromBody] AdminLocalizedListSettingUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateLocalizedListAsync(key, dto, cancellationToken));

    [HttpGet("terms")]
    public async Task<IActionResult> GetTerms(CancellationToken cancellationToken)
        => Ok(await service.GetTermsAsync(cancellationToken));

    [HttpPut("terms")]
    public async Task<IActionResult> UpdateTerms([FromBody] AdminTermsUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateTermsAsync(dto, cancellationToken));

    [HttpGet("guest-permissions")]
    public async Task<IActionResult> GetGuestPermissions(CancellationToken cancellationToken)
        => Ok(await service.GetGuestPermissionsAsync(cancellationToken));

    [HttpPut("guest-permissions")]
    public async Task<IActionResult> UpdateGuestPermissions([FromBody] AdminGuestPermissionsUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateGuestPermissionsAsync(dto, cancellationToken));

    [HttpGet("branding")]
    public async Task<IActionResult> GetBranding(CancellationToken cancellationToken)
        => Ok(await service.GetBrandingAsync(cancellationToken));

    [HttpPut("branding")]
    public async Task<IActionResult> UpdateBranding([FromBody] AdminBrandingUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateBrandingAsync(dto, cancellationToken));

    [HttpGet("quick-access-tiles")]
    public async Task<IActionResult> GetQuickAccessTiles(CancellationToken cancellationToken)
        => Ok(await service.GetQuickAccessTilesAsync(cancellationToken));

    [HttpPut("quick-access-tiles")]
    public async Task<IActionResult> UpdateQuickAccessTiles([FromBody] AdminQuickAccessTilesUpdateDto dto, CancellationToken cancellationToken)
        => Ok(await service.UpdateQuickAccessTilesAsync(dto, cancellationToken));
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/files")]
public class FilesAdminController(IFileStorageService fileStorage) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        await using var stream = file.OpenReadStream();
        var result = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(result);
    }
}
