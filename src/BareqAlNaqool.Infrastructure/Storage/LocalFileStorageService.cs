using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BareqAlNaqool.Infrastructure.Storage;

public class LocalFileStorageService(IOptions<StorageOptions> options, IHttpClientFactory httpClientFactory) : IFileStorageService
{
    private readonly StorageOptions _options = options.Value;

    public async Task<FileUploadResultDto> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var root = Path.GetFullPath(_options.RootPath);
        Directory.CreateDirectory(root);
        var fullPath = Path.Combine(root, storedName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        var url = $"{_options.PublicBasePath.TrimEnd('/')}/{storedName}";
        var size = new FileInfo(fullPath).Length;
        return new FileUploadResultDto(url, storedName, size);
    }

    public async Task<Stream?> OpenReadAsync(string url, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(url);
        var fullPath = Path.Combine(Path.GetFullPath(_options.RootPath), fileName);
        if (File.Exists(fullPath))
        {
            return File.OpenRead(fullPath);
        }

        if (!string.IsNullOrWhiteSpace(_options.AdminFilesBaseUrl))
        {
            var remoteUrl = $"{_options.AdminFilesBaseUrl.TrimEnd('/')}{_options.PublicBasePath.TrimEnd('/')}/{fileName}";
            using var client = httpClientFactory.CreateClient(nameof(LocalFileStorageService));
            using var response = await client.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return new MemoryStream(bytes);
            }
        }

        return null;
    }

    public string GetPublicPath(string storedPath) => storedPath;
}
