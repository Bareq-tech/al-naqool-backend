namespace BareqAlNaqool.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string RootPath { get; set; } = "uploads";

    public string PublicBasePath { get; set; } = "/files";
}
