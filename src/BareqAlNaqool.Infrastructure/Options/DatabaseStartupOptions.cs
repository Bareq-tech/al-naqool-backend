namespace BareqAlNaqool.Infrastructure.Options;

public class DatabaseStartupOptions
{
    public const string SectionName = "Database";

    public bool MigrateOnStartup { get; set; }

    public bool SeedOnStartup { get; set; }
}
