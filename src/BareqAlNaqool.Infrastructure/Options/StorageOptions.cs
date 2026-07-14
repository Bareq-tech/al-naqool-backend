namespace BareqAlNaqool.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string RootPath { get; set; } = "uploads";

    public string PublicBasePath { get; set; } = "/files";

    /// <summary>
    /// When set (e.g. mobile API pointing at admin API), file reads fall back to this host if the file is not on local disk.
    /// </summary>
    public string? AdminFilesBaseUrl { get; set; }
}
