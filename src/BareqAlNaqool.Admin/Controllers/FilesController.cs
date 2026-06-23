using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Admin.Controllers;

[ApiController]
[AllowAnonymous]
[Route("files")]
public class FilesController(IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Download(string fileName, CancellationToken cancellationToken)
    {
        var stream = await fileStorage.OpenReadAsync($"/files/{fileName}", cancellationToken);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", fileName);
    }
}
