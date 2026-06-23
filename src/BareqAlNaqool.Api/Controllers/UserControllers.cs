using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Api.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.LoginAsync(request, cancellationToken));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        => Ok(await authService.RegisterAsync(request, cancellationToken));

    [HttpGet("terms")]
    [AllowAnonymous]
    public async Task<IActionResult> Terms(CancellationToken cancellationToken)
        => Ok(await authService.GetTermsAsync(cancellationToken));

    [HttpPost("guest")]
    [AllowAnonymous]
    public async Task<IActionResult> Guest(CancellationToken cancellationToken)
        => Ok(await authService.ContinueAsGuestAsync(cancellationToken));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (CurrentUserId.HasValue)
        {
            await authService.LogoutAsync(CurrentUserId.Value, cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request.Email, cancellationToken);
        return NoContent();
    }

    [HttpGet("session")]
    [Authorize]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        var session = await authService.GetSessionAsync(CurrentUserId, cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }
}

[Route("api/profile")]
[Authorize]
public class ProfileController(IProfileService profileService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await profileService.GetProfileAsync(CurrentUserId.Value, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await profileService.UpdateProfileAsync(CurrentUserId.Value, request, cancellationToken));
    }
}

[Route("api/directory")]
public class DirectoryController(IDirectoryService directoryService) : ApiControllerBase
{
    [HttpGet("branches")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
        => Ok(await directoryService.GetBranchesAsync(cancellationToken));

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMembers([FromQuery] string? query, [FromQuery] string? branchId, CancellationToken cancellationToken)
        => Ok(await directoryService.GetMembersAsync(query, branchId, cancellationToken));

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMember(string id, CancellationToken cancellationToken)
    {
        var item = await directoryService.GetMemberByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}

[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationsService notificationsService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await notificationsService.GetNotificationsAsync(CurrentUserId.Value, cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await notificationsService.GetUnreadCountAsync(CurrentUserId.Value, cancellationToken));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(string id, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        await notificationsService.MarkAsReadAsync(id, CurrentUserId.Value, cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        await notificationsService.MarkAllAsReadAsync(CurrentUserId.Value, cancellationToken);
        return NoContent();
    }
}

[Route("api/contact")]
public class ContactController(IContactService contactService) : ApiControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] ContactRequestDto request, CancellationToken cancellationToken)
    {
        await contactService.SubmitContactAsync(request, cancellationToken);
        return NoContent();
    }
}
