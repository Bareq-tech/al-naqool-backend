using BareqAlNaqool.Infrastructure.Options;
using BareqAlNaqool.Infrastructure.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class DatabaseStartup
{
    public static async Task ApplyConfiguredStartupAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var options = configuration.GetSection(DatabaseStartupOptions.SectionName).Get<DatabaseStartupOptions>()
            ?? new DatabaseStartupOptions();

        if (!options.MigrateOnStartup && !options.SeedOnStartup)
        {
            return;
        }

        if (options.MigrateOnStartup)
        {
            logger.LogInformation("Applying EF Core migrations on startup.");
            await DataSeeder.MigrateAsync(services);
        }

        if (options.SeedOnStartup)
        {
            logger.LogInformation("Seeding database on startup.");
            await DataSeeder.SeedAsync(services);
        }
    }
}
