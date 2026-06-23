using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BareqAlNaqool.Infrastructure.Persistence;

public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration, string name = "DefaultConnection")
    {
        if (IsProduction())
        {
            return ResolveProduction(configuration);
        }

        return ResolveDevelopment(configuration, name);
    }

    private static string ResolveProduction(IConfiguration configuration)
    {
        var databaseUrl = GetDatabaseUrl(configuration);
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ParseDatabaseUrl(databaseUrl);
        }

        var fromPgVariables = TryBuildFromPgVariables();
        if (fromPgVariables is not null)
        {
            return fromPgVariables;
        }

        throw new InvalidOperationException(
            "DATABASE_URL must be set in Production. " +
            "On Railway, add DATABASE_URL=${{ Postgres.DATABASE_URL }} to this service. " +
            "Remove ConnectionStrings__DefaultConnection if you copied local dev values.");
    }

    private static string ResolveDevelopment(IConfiguration configuration, string name)
    {
        var databaseUrl = GetDatabaseUrl(configuration);
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ParseDatabaseUrl(databaseUrl);
        }

        var fromPgVariables = TryBuildFromPgVariables();
        if (fromPgVariables is not null)
        {
            return fromPgVariables;
        }

        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found. Set DATABASE_URL or ConnectionStrings__{name}.");
        }

        return connectionString;
    }

    private static string? GetDatabaseUrl(IConfiguration configuration)
    {
        return Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration["DATABASE_URL"];
    }

    private static string? TryBuildFromPgVariables()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST")
            ?? Environment.GetEnvironmentVariable("POSTGRES_HOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(
                Environment.GetEnvironmentVariable("PGPORT") ?? Environment.GetEnvironmentVariable("POSTGRES_PORT"),
                out var port)
                ? port
                : 5432,
            Database = Environment.GetEnvironmentVariable("PGDATABASE")
                ?? Environment.GetEnvironmentVariable("POSTGRES_DB")
                ?? string.Empty,
            Username = Environment.GetEnvironmentVariable("PGUSER")
                ?? Environment.GetEnvironmentVariable("POSTGRES_USER")
                ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("PGPASSWORD")
                ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                ?? string.Empty
        };

        if (ShouldRequireSsl(host))
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    private static string ParseDatabaseUrl(string databaseUrl)
    {
        var normalized = databaseUrl.Trim();

        if (!normalized.Contains("://", StringComparison.Ordinal))
        {
            return normalized;
        }

        var uri = new Uri(normalized);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/').Split('?')[0],
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty
        };

        if (ShouldRequireSsl(uri.Host))
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    private static bool ShouldRequireSsl(string host)
    {
        return !IsLocalHost(host);
    }

    private static bool IsLocalHost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || host.Equals("postgres", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProduction()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            Environments.Production,
            StringComparison.OrdinalIgnoreCase);
    }
}
