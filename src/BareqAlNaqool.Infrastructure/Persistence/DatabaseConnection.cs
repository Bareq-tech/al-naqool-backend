using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BareqAlNaqool.Infrastructure.Persistence;

public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration, string name = "DefaultConnection")
    {
        var raw = GetRawConnectionString(configuration, name);

        if (IsPlaceholder(raw))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' is missing or still a placeholder. " +
                "On Railway, set ConnectionStrings__DefaultConnection=${{ Postgres.DATABASE_URL }} " +
                "(reference your Postgres service — same pattern as Midank WebApi).");
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found. " +
                "Set ConnectionStrings__DefaultConnection locally, or on Railway use " +
                "ConnectionStrings__DefaultConnection=${{ Postgres.DATABASE_URL }}.");
        }

        return NpgsqlConnectionStringHelper.Normalize(raw);
    }

    public static string Describe(IConfiguration configuration, string name = "DefaultConnection")
    {
        var raw = GetRawConnectionString(configuration, name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "source=none";
        }

        var builder = new NpgsqlConnectionStringBuilder(NpgsqlConnectionStringHelper.Normalize(raw));
        var source = !string.IsNullOrWhiteSpace(configuration.GetConnectionString(name))
            ? $"ConnectionStrings:{name}"
            : "DATABASE_URL";
        return $"source={source}, host={builder.Host}, database={builder.Database}, user={builder.Username}";
    }

    public static async Task VerifyConnectivityAsync(
        string connectionString,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            logger?.LogInformation("Database connection verified.");
        }
        catch (PostgresException ex) when (ex.SqlState == "28P01")
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            throw new InvalidOperationException(
                $"PostgreSQL password authentication failed for user '{builder.Username}' on host '{builder.Host}'. " +
                "On Railway use ConnectionStrings__DefaultConnection=${{ Postgres.DATABASE_URL }} with no quotes. " +
                "Remove any manually typed connection string or dev password.",
                ex);
        }
    }

    private static string? GetRawConnectionString(IConfiguration configuration, string name)
    {
        var fromConfig = SanitizeValue(configuration.GetConnectionString(name));
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            return fromConfig;
        }

        // Backward compatibility if DATABASE_URL is set instead of ConnectionStrings__DefaultConnection.
        return SanitizeValue(Environment.GetEnvironmentVariable("DATABASE_URL"))
            ?? SanitizeValue(configuration["DATABASE_URL"]);
    }

    private static bool IsPlaceholder(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains("#{", StringComparison.Ordinal);

    private static string? SanitizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        if (trimmed.Length >= 2 && trimmed.StartsWith('\'') && trimmed.EndsWith('\''))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
