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

        if (databaseStartup.SeedOnStartup)
        {
            throw new InvalidOperationException(
                "Database__SeedOnStartup must not be enabled in Production after the first deploy. " +
                "Initial seed runs automatically when the database is empty.");
        }
    }

    private static void ValidateDatabaseConfiguration(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection must be set in Production. " +
                "On Railway: ConnectionStrings__DefaultConnection=${{ Postgres.DATABASE_URL }} " +
                "(use Add Reference, not a typed literal).");
        }

        if (raw.Contains("${{", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection is an unresolved Railway template. " +
                "Use Railway's 'Add Reference' to link Postgres.DATABASE_URL.");
        }

        if (raw.Contains("#{", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection is still a placeholder. " +
                "Set it on Railway using the Postgres service reference.");
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
