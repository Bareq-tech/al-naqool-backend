using BareqAlNaqool.Infrastructure.Options;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class ProductionConfiguration
{
    private const int MinimumJwtKeyLength = 32;

    private static readonly string[] KnownDevJwtKeys =
    [
        "BareqAlNaqoolDevSecretKeyChangeInProduction!",
        "BareqAlNaqoolMobileApiSecretKey2024!",
        "BareqAlNaqoolAdminApiSecretKey2024!"
    ];

    public static void ValidateProductionSettings(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        ValidateDatabaseConfiguration(builder.Configuration);
        ValidateDatabaseStartupConfiguration(builder.Configuration);
        ValidateJwtConfiguration(builder.Configuration);
    }

    private static void ValidateDatabaseStartupConfiguration(IConfiguration configuration)
    {
        var databaseStartup = configuration.GetSection(DatabaseStartupOptions.SectionName).Get<DatabaseStartupOptions>();
        if (databaseStartup is null)
        {
            return;
        }

        if (databaseStartup.MigrateOnStartup || databaseStartup.SeedOnStartup)
        {
            throw new InvalidOperationException(
                "Database__MigrateOnStartup and Database__SeedOnStartup must not be enabled in Production. " +
                "Remove them from Railway and run migrations with Dockerfile.migrate instead.");
        }
    }

    private static void ValidateDatabaseConfiguration(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration["DATABASE_URL"];
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");

        if (string.IsNullOrWhiteSpace(databaseUrl) && string.IsNullOrWhiteSpace(pgHost))
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings__DefaultConnection is set but DATABASE_URL is missing in Production. " +
                    "Remove ConnectionStrings__DefaultConnection from Railway and add " +
                    "DATABASE_URL=${{ Postgres.DATABASE_URL }} instead.");
            }

            throw new InvalidOperationException(
                "DATABASE_URL must be set in Production. " +
                "Add DATABASE_URL=${{ Postgres.DATABASE_URL }} to your Railway service.");
        }

        _ = DatabaseConnection.Resolve(configuration);
    }

    private static void ValidateJwtConfiguration(IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwt is null || string.IsNullOrWhiteSpace(jwt.Key))
        {
            throw new InvalidOperationException(
                "Jwt__Key must be set in Production. Use a long random secret (at least 32 characters).");
        }

        if (jwt.Key.Length < MinimumJwtKeyLength)
        {
            throw new InvalidOperationException(
                $"Jwt__Key must be at least {MinimumJwtKeyLength} characters in Production.");
        }

        if (KnownDevJwtKeys.Contains(jwt.Key, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "Jwt__Key is using a development default. Set a unique production secret via Railway variables.");
        }
    }
}
