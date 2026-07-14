using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Admin.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/documents")]
public class DocumentsAdminController(
    IAdminCrudService<AdminDocumentDto, AdminDocumentCreateDto, AdminDocumentUpdateDto> service,
    IFileStorageService fileStorage) : ControllerBase
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
    [RequestSizeLimit(52_428_800)]
    [Consumes("multipart/form-data", "application/json")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        AdminDocumentCreateDto dto;
        if (Request.HasFormContentType)
        {
            var formResult = await BindCreateFormAsync(cancellationToken);
            if (formResult.Error is not null)
            {
                return BadRequest(formResult.Error);
            }

            dto = formResult.Dto!;
        }
        else
        {
            dto = await Request.ReadFromJsonAsync<AdminDocumentCreateDto>(cancellationToken)
                ?? throw new BadHttpRequestException("Request body is required.");

            var validation = await ValidateStoredFileAsync(dto.FileUrl, cancellationToken);
            if (validation is not null)
            {
                return BadRequest(validation);
            }

            dto = dto with { FileUrl = NormalizeFileUrl(dto.FileUrl)! };
        }

        return Ok(await service.CreateAsync(dto, cancellationToken));
    }

    [HttpPut("{id}")]
    [RequestSizeLimit(52_428_800)]
    [Consumes("multipart/form-data", "application/json")]
    public async Task<IActionResult> Update(string id, CancellationToken cancellationToken)
    {
        AdminDocumentUpdateDto dto;
        if (Request.HasFormContentType)
        {
            var formResult = await BindUpdateFormAsync(cancellationToken);
            if (formResult.Error is not null)
            {
                return BadRequest(formResult.Error);
            }

            dto = formResult.UpdateDto!;
        }
        else
        {
            dto = await Request.ReadFromJsonAsync<AdminDocumentUpdateDto>(cancellationToken)
                ?? throw new BadHttpRequestException("Request body is required.");

            if (!string.IsNullOrWhiteSpace(dto.FileUrl))
            {
                var validation = await ValidateStoredFileAsync(dto.FileUrl, cancellationToken);
                if (validation is not null)
                {
                    return BadRequest(validation);
                }

                dto = dto with { FileUrl = NormalizeFileUrl(dto.FileUrl)! };
            }
        }

        var item = await service.UpdateAsync(id, dto, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private async Task<(AdminDocumentCreateDto? Dto, string? Error)> BindCreateFormAsync(CancellationToken cancellationToken)
    {
        var form = Request.Form;
        var file = GetUploadedFile();
        var fileUrl = form["fileUrl"].FirstOrDefault() ?? string.Empty;
        var fileSize = form["fileSize"].FirstOrDefault() ?? string.Empty;

        if (file is { Length: > 0 })
        {
            await using var stream = file.OpenReadStream();
            var upload = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
            fileUrl = upload.Url;
            fileSize = FormatFileSize(upload.SizeBytes);
        }
        else
        {
            var validation = await ValidateStoredFileAsync(fileUrl, cancellationToken);
            if (validation is not null)
            {
                return (null, validation);
            }

            fileUrl = NormalizeFileUrl(fileUrl)!;
        }

        return (new AdminDocumentCreateDto(
            form["titleEn"].FirstOrDefault() ?? string.Empty,
            form["titleAr"].FirstOrDefault() ?? string.Empty,
            fileSize,
            form["dateEn"].FirstOrDefault() ?? string.Empty,
            form["dateAr"].FirstOrDefault() ?? string.Empty,
            form["category"].FirstOrDefault() ?? string.Empty,
            form["descriptionEn"].FirstOrDefault() ?? string.Empty,
            form["descriptionAr"].FirstOrDefault() ?? string.Empty,
            fileUrl,
            form["accessLevel"].FirstOrDefault() ?? "Public"), null);
    }

    private async Task<(AdminDocumentUpdateDto? UpdateDto, string? Error)> BindUpdateFormAsync(CancellationToken cancellationToken)
    {
        var form = Request.Form;
        var file = GetUploadedFile();
        var fileUrl = form["fileUrl"].FirstOrDefault() ?? string.Empty;
        var fileSize = form["fileSize"].FirstOrDefault() ?? string.Empty;

        if (file is { Length: > 0 })
        {
            await using var stream = file.OpenReadStream();
            var upload = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
            fileUrl = upload.Url;
            fileSize = FormatFileSize(upload.SizeBytes);
        }
        else if (!string.IsNullOrWhiteSpace(fileUrl))
        {
            var validation = await ValidateStoredFileAsync(fileUrl, cancellationToken);
            if (validation is not null)
            {
                return (null, validation);
            }

            fileUrl = NormalizeFileUrl(fileUrl)!;
        }

        return (new AdminDocumentUpdateDto(
            form["titleEn"].FirstOrDefault() ?? string.Empty,
            form["titleAr"].FirstOrDefault() ?? string.Empty,
            fileSize,
            form["dateEn"].FirstOrDefault() ?? string.Empty,
            form["dateAr"].FirstOrDefault() ?? string.Empty,
            form["category"].FirstOrDefault() ?? string.Empty,
            form["descriptionEn"].FirstOrDefault() ?? string.Empty,
            form["descriptionAr"].FirstOrDefault() ?? string.Empty,
            fileUrl,
            form["accessLevel"].FirstOrDefault() ?? "Public"), null);
    }

    private IFormFile? GetUploadedFile()
    {
        foreach (var file in Request.Form.Files)
        {
            if (string.Equals(file.Name, "file", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(file.Name, "documentFile", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(file.Name, "document", StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return Request.Form.Files.FirstOrDefault();
    }

    private async Task<string?> ValidateStoredFileAsync(string? fileUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return "A document file is required. Attach a file to the form or upload via POST /api/admin/files/upload and use the returned url as fileUrl.";
        }

        var normalized = NormalizeFileUrl(fileUrl);
        if (normalized is null)
        {
            return "fileUrl must be the path returned from POST /api/admin/files/upload (e.g. /files/abc123.pdf). The original filename cannot be used.";
        }

        await using var stream = await fileStorage.OpenReadAsync(normalized, cancellationToken);
        if (stream is null)
        {
            return $"No file found at '{normalized}'. Upload the file first via POST /api/admin/files/upload or attach it to this request.";
        }

        return null;
    }

    private static string? NormalizeFileUrl(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        if (fileUrl.StartsWith("/files/", StringComparison.Ordinal))
        {
            return fileUrl;
        }

        var fileName = Path.GetFileName(fileUrl);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return $"/files/{fileName}";
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.#} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):0.#} MB";
    }
}
