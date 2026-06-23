using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Api.Controllers;

[Route("api/messages")]
[Authorize]
public class MessagesController(IMessagesService messagesService) : ApiControllerBase
{
    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters(CancellationToken cancellationToken)
        => Ok(await messagesService.GetFilterTypesAsync(cancellationToken));

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] string? type, CancellationToken cancellationToken)
        => Ok(await messagesService.GetConversationsAsync(type, CurrentUserId, cancellationToken));

    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(string id, CancellationToken cancellationToken)
    {
        var item = await messagesService.GetConversationByIdAsync(id, CurrentUserId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetMessages(string id, CancellationToken cancellationToken)
        => Ok(await messagesService.GetChatMessagesAsync(id, CurrentUserId, cancellationToken));

    [HttpPost("conversations/{id}/messages")]
    public async Task<IActionResult> SendMessage(string id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized();
        }

        await messagesService.SendMessageAsync(id, request.Content, CurrentUserId.Value, cancellationToken);
        return NoContent();
    }
}

[Route("api/gallery")]
public class GalleryController(IGalleryService galleryService) : ApiControllerBase
{
    [HttpGet("types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
        => Ok(await galleryService.GetGalleryTypesAsync(cancellationToken));

    [HttpGet("albums")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAlbums([FromQuery] string? type, CancellationToken cancellationToken)
        => Ok(await galleryService.GetAlbumsAsync(type, cancellationToken));

    [HttpGet("albums/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAlbum(string id, CancellationToken cancellationToken)
    {
        var item = await galleryService.GetAlbumByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("albums/{id}/photos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhotos(string id, CancellationToken cancellationToken)
        => Ok(await galleryService.GetAlbumPhotosAsync(id, cancellationToken));
}

[Route("api/documents")]
public class DocumentsController(IDocumentsService documentsService, IFileStorageService fileStorage) : ApiControllerBase
{
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        => Ok(await documentsService.GetCategoriesAsync(cancellationToken));

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocuments([FromQuery] string? category, CancellationToken cancellationToken)
        => Ok(await documentsService.GetDocumentsAsync(category, cancellationToken));

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await documentsService.GetDocumentByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(string id, CancellationToken cancellationToken)
    {
        var item = await documentsService.GetDocumentByIdAsync(id, cancellationToken);
        if (item is null || string.IsNullOrWhiteSpace(item.FileUrl))
        {
            return NotFound();
        }

        var stream = await fileStorage.OpenReadAsync(item.FileUrl, cancellationToken);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", Path.GetFileName(item.FileUrl));
    }
}

[Route("api/council")]
public class CouncilController(ICouncilService councilService) : ApiControllerBase
{
    [HttpGet("modules")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModules(CancellationToken cancellationToken)
        => Ok(await councilService.GetModulesAsync(cancellationToken));

    [HttpGet("latest-meeting")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLatestMeeting(CancellationToken cancellationToken)
        => Ok(await councilService.GetLatestMeetingAsync(cancellationToken));

    [HttpGet("president")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPresident(CancellationToken cancellationToken)
        => Ok(new { name = await councilService.GetPresidentNameAsync(cancellationToken) });

    [HttpGet("{moduleId}/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModuleItems(string moduleId, CancellationToken cancellationToken)
        => Ok(await councilService.GetModuleItemsAsync(moduleId, cancellationToken));
}
