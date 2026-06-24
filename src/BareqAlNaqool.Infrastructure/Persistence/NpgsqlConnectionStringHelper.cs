using Npgsql;

namespace BareqAlNaqool.Infrastructure.Persistence;

public static class NpgsqlConnectionStringHelper
{
    public static string Normalize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString ?? string.Empty;
        }

        var sanitized = SanitizeValue(connectionString);
        var builder = sanitized.Contains("://", StringComparison.Ordinal)
            ? ParseDatabaseUri(sanitized)
            : new NpgsqlConnectionStringBuilder(sanitized);

        builder.SslMode = ShouldRequireSsl(builder.Host ?? string.Empty)
            ? SslMode.Prefer
            : builder.SslMode;

        return builder.ConnectionString;
    }

    private static NpgsqlConnectionStringBuilder ParseDatabaseUri(string databaseUrl)
    {
        var schemeEnd = databaseUrl.IndexOf("://", StringComparison.Ordinal) + 3;
        var remainder = databaseUrl[schemeEnd..];
        var atIndex = remainder.LastIndexOf('@');
        if (atIndex < 0)
        {
            throw new InvalidOperationException("DATABASE_URL is malformed: missing '@' between credentials and host.");
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

        return new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = parsedPort,
            Database = database,
            Username = username,
            Password = password
        };
    }

    private static bool ShouldRequireSsl(string host)
    {
        return !host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            && !host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            && !host.Equals("postgres", StringComparison.OrdinalIgnoreCase)
            && !host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase);
    }

    private static string SanitizeValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        if (trimmed.Length >= 2 && trimmed.StartsWith('\'') && trimmed.EndsWith('\''))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}
