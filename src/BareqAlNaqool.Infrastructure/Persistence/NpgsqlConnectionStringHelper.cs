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

        var builder = new NpgsqlConnectionStringBuilder(SanitizeValue(connectionString))
        {
            SslMode = SslMode.Prefer
        };

        return builder.ConnectionString;
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
