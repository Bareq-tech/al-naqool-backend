using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Api.Controllers;

[Route("api/news")]
public class NewsController(INewsService newsService) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetNews([FromQuery] string? category, CancellationToken cancellationToken)
        => Ok(await newsService.GetNewsAsync(category, cancellationToken));

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        => Ok(await newsService.GetCategoriesAsync(cancellationToken));

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await newsService.GetNewsByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}

[Route("api/events")]
[Authorize]
public class EventsController(IEventsService eventsService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] string filter = "All", CancellationToken cancellationToken = default)
        => Ok(await eventsService.GetEventsAsync(filter, CurrentUserId, cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await eventsService.GetEventByIdAsync(id, CurrentUserId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id}/registered")]
    public async Task<IActionResult> IsRegistered(string id, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        return Ok(await eventsService.IsRegisteredAsync(id, CurrentUserId.Value, cancellationToken));
    }

    [HttpPost("{id}/register")]
    public async Task<IActionResult> Register(string id, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        await eventsService.RegisterForEventAsync(id, CurrentUserId.Value, cancellationToken);
        return NoContent();
    }
}

[Route("api/family-tree")]
public class FamilyTreeController(IFamilyTreeService familyTreeService) : ApiControllerBase
{
    [HttpGet("branches")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
        => Ok(await familyTreeService.GetBranchesAsync(cancellationToken));

    [HttpGet("branches/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranch(string id, CancellationToken cancellationToken)
    {
        var item = await familyTreeService.GetBranchByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("branches/{id}/members")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranchMembers(string id, CancellationToken cancellationToken)
        => Ok(await familyTreeService.GetBranchMembersAsync(id, cancellationToken));

    [HttpGet("founder-lineage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFounderLineage(CancellationToken cancellationToken)
        => Ok(await familyTreeService.GetFounderLineageAsync(cancellationToken));
}
