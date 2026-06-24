using BareqAlNaqool.Infrastructure.Options;
using BareqAlNaqool.Infrastructure.Persistence;
using BareqAlNaqool.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class DatabaseStartup
{
    public static async Task ApplyConfiguredStartupAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger,
        IHostEnvironment? environment = null)
    {
        environment ??= services.GetRequiredService<IHostEnvironment>();
        var options = configuration.GetSection(DatabaseStartupOptions.SectionName).Get<DatabaseStartupOptions>()
            ?? new DatabaseStartupOptions();

        var shouldMigrate = options.MigrateOnStartup || environment.IsProduction();
        var shouldSeed = options.SeedOnStartup;

        if (shouldMigrate)
        {
            logger.LogInformation("Applying EF Core migrations on startup.");
            await MigrateWithRetryAsync(services, logger);
        }

        if (shouldSeed)
        {
            logger.LogInformation("Seeding database on startup.");
            await DataSeeder.SeedAsync(services);
            return;
        }

        if (environment.IsProduction() && await NeedsInitialSeedAsync(services))
        {
            logger.LogInformation("Empty production database detected — running initial seed.");
            await DataSeeder.SeedAsync(services);
        }
    }

    private static async Task MigrateWithRetryAsync(IServiceProvider services, ILogger logger, int maxAttempts = 6)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await DataSeeder.MigrateAsync(services);
                return;
            }
            catch (NpgsqlException ex) when (attempt < maxAttempts && ex.IsTransient)
            {
                logger.LogWarning(
                    ex,
                    "PostgreSQL not reachable yet (attempt {Attempt}/{MaxAttempts}). Retrying in 10 seconds...",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }

    private static async Task<bool> NeedsInitialSeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return !await db.FamilyBranches.AnyAsync();
    }
}
