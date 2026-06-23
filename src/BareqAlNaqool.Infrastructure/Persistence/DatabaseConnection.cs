using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BareqAlNaqool.Infrastructure.Persistence;

public static class DatabaseConnection
{
    private const string DevDatabaseName = "bareq_alnaqool";

    public static string Resolve(IConfiguration configuration, string name = "DefaultConnection")
    {
        var (connectionString, source) = ResolveInternal(configuration, name);
        ValidateProductionTarget(connectionString, source);
        return connectionString;
    }

    public static string Describe(IConfiguration configuration, string name = "DefaultConnection")
    {
        var (connectionString, source) = ResolveInternal(configuration, name);
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"source={source}, host={builder.Host}, database={builder.Database}, user={builder.Username}";
    }

    private static (string ConnectionString, string Source) ResolveInternal(
        IConfiguration configuration,
        string name)
    {
        if (IsProduction())
        {
            return ResolveProduction(configuration);
        }

        return ResolveDevelopment(configuration, name);
    }

    private static (string ConnectionString, string Source) ResolveProduction(IConfiguration configuration)
    {
        var fromPgVariables = TryBuildFromPgVariables();
        if (fromPgVariables is not null)
        {
            return (fromPgVariables, "PGHOST/PGUSER/PGPASSWORD");
        }

        var databaseUrl = GetDatabaseUrl(configuration);
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return (ParseDatabaseUrl(databaseUrl), "DATABASE_URL");
        }

        throw new InvalidOperationException(
            "DATABASE_URL must be set in Production. " +
            "On Railway, use DATABASE_URL=${{ Postgres.DATABASE_URL }} (reference your Postgres service). " +
            "Remove ConnectionStrings__DefaultConnection and any manually typed postgres:// URL.");
    }

    private static (string ConnectionString, string Source) ResolveDevelopment(
        IConfiguration configuration,
        string name)
    {
        var fromPgVariables = TryBuildFromPgVariables();
        if (fromPgVariables is not null)
        {
            return (fromPgVariables, "PGHOST/PGUSER/PGPASSWORD");
        }

        var databaseUrl = GetDatabaseUrl(configuration);
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return (ParseDatabaseUrl(databaseUrl), "DATABASE_URL");
        }

        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found. Set DATABASE_URL or ConnectionStrings__{name}.");
        }

        return (connectionString, $"ConnectionStrings:{name}");
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
        var password = Environment.GetEnvironmentVariable("PGPASSWORD")
            ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(password))
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
            Password = password
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

        var schemeEnd = normalized.IndexOf("://", StringComparison.Ordinal) + 3;
        var remainder = normalized[schemeEnd..];
        var atIndex = remainder.LastIndexOf('@');
        if (atIndex < 0)
        {
            throw new InvalidOperationException("DATABASE_URL is malformed.");
        }

        var credentials = remainder[..atIndex];
        var hostPart = remainder[(atIndex + 1)..];

        var colonIndex = credentials.IndexOf(':');
        var username = colonIndex >= 0
            ? Uri.UnescapeDataString(credentials[..colonIndex])
            : Uri.UnescapeDataString(credentials);
        var password = colonIndex >= 0
            ? Uri.UnescapeDataString(credentials[(colonIndex + 1)..])
            : string.Empty;

        var queryIndex = hostPart.IndexOf('?');
        if (queryIndex >= 0)
        {
            hostPart = hostPart[..queryIndex];
        }

        var slashIndex = hostPart.IndexOf('/');
        var hostAndPort = slashIndex >= 0 ? hostPart[..slashIndex] : hostPart;
        var database = slashIndex >= 0 ? hostPart[(slashIndex + 1)..] : string.Empty;

        var portColonIndex = hostAndPort.LastIndexOf(':');
        var host = portColonIndex >= 0 ? hostAndPort[..portColonIndex] : hostAndPort;
        var parsedPort = portColonIndex >= 0
            && int.TryParse(hostAndPort[(portColonIndex + 1)..], out var port)
            ? port
            : 5432;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = parsedPort,
            Database = database,
            Username = username,
            Password = password
        };

        if (ShouldRequireSsl(host))
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    private static void ValidateProductionTarget(string connectionString, string source)
    {
        if (!IsProduction())
        {
            return;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = builder.Host ?? string.Empty;
        var database = builder.Database ?? string.Empty;

        if (database.Equals(DevDatabaseName, StringComparison.OrdinalIgnoreCase)
            && (host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase)
                || host.Equals("postgres", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Database '{DevDatabaseName}' is a local dev name but host '{host}' is Railway. " +
                $"Your {source} was likely typed manually. Delete DATABASE_URL on this service and re-add it as " +
                "a reference: DATABASE_URL=${{ Postgres.DATABASE_URL }}. " +
                "The Railway database name is usually 'railway', not 'bareq_alnaqool'.");
        }
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
