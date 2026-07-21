using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Admin.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetStatsAsync(cancellationToken));
}

[Route("api/admin/news")]
public class NewsAdminController(IAdminCrudService<AdminNewsDto, AdminNewsCreateDto, AdminNewsUpdateDto> service) : AdminCrudController<AdminNewsDto, AdminNewsCreateDto, AdminNewsUpdateDto>(service);

[Route("api/admin/events")]
public class EventsAdminController(IAdminCrudService<AdminEventDto, AdminEventCreateDto, AdminEventUpdateDto> service) : AdminCrudController<AdminEventDto, AdminEventCreateDto, AdminEventUpdateDto>(service);

[Route("api/admin/branches")]
public class BranchesAdminController(IAdminCrudService<AdminBranchDto, AdminBranchCreateDto, AdminBranchUpdateDto> service) : AdminCrudController<AdminBranchDto, AdminBranchCreateDto, AdminBranchUpdateDto>(service);

[Route("api/admin/albums")]
public class AlbumsAdminController(IAdminCrudService<AdminAlbumDto, AdminAlbumCreateDto, AdminAlbumUpdateDto> service) : AdminCrudController<AdminAlbumDto, AdminAlbumCreateDto, AdminAlbumUpdateDto>(service);

[Route("api/admin/notifications")]
public class NotificationsAdminController(IAdminCrudService<AdminNotificationDto, AdminNotificationCreateDto, AdminNotificationUpdateDto> service) : AdminCrudController<AdminNotificationDto, AdminNotificationCreateDto, AdminNotificationUpdateDto>(service);

[Route("api/admin/council-items")]
public class CouncilItemsAdminController(IAdminCrudService<AdminCouncilItemDto, AdminCouncilItemCreateDto, AdminCouncilItemUpdateDto> service) : AdminCrudController<AdminCouncilItemDto, AdminCouncilItemCreateDto, AdminCouncilItemUpdateDto>(service);

[Route("api/admin/directory-members")]
[Authorize(Policy = "AdminOnly")]
[ApiController]
public class DirectoryMembersAdminController(IAdminCrudService<AdminDirectoryMemberDto, AdminDirectoryMemberCreateDto, AdminDirectoryMemberUpdateDto> service)
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
    public async Task<IActionResult> Create([FromBody] AdminDirectoryMemberCreateDto dto, CancellationToken cancellationToken)
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
    public async Task<IActionResult> Update(string id, [FromBody] AdminDirectoryMemberUpdateDto dto, CancellationToken cancellationToken)
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

[Route("api/admin/users")]
public class UsersAdminController(IAdminCrudService<AdminUserDto, AdminUserCreateDto, AdminUserUpdateDto> service) : AdminCrudController<AdminUserDto, AdminUserCreateDto, AdminUserUpdateDto>(service);

[ApiController]
[Authorize(Policy = "AdminOnly")]
public abstract class AdminCrudController<TDto, TCreateDto, TUpdateDto>(IAdminCrudService<TDto, TCreateDto, TUpdateDto> service) : ControllerBase
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
  public async Task<IActionResult> Create([FromBody] TCreateDto dto, CancellationToken cancellationToken)
  {
    var created = await service.CreateAsync(dto, cancellationToken);
    return Ok(created);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(string id, [FromBody] TUpdateDto dto, CancellationToken cancellationToken)
  {
    var item = await service.UpdateAsync(id, dto, cancellationToken);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
