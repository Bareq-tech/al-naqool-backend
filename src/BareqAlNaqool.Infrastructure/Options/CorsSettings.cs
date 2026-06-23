namespace BareqAlNaqool.Infrastructure.Options;

public class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "BareqCors";

    /// <summary>
    /// Comma-separated list of allowed origins, e.g. "https://admin.example.com,https://app.example.com".
    /// Leave empty to disable CORS (fine for native mobile clients).
    /// </summary>
    public string AllowedOrigins { get; set; } = string.Empty;

    public IReadOnlyList<string> GetOrigins()
    {
        return AllowedOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
