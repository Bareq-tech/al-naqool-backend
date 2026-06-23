using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BareqAlNaqool.Infrastructure.Persistence;

public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration, string name = "DefaultConnection")
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ParseDatabaseUrl(databaseUrl);
        }

        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found. Set DATABASE_URL or ConnectionStrings__{name}.");
        }

        if (IsProduction() && PointsToLocalHost(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' points to localhost in Production. " +
                "Set DATABASE_URL to your PostgreSQL instance.");
        }

        return connectionString;
    }

    private static string ParseDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
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

    private static bool PointsToLocalHost(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return IsLocalHost(builder.Host ?? string.Empty);
        }
        catch
        {
            return connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool IsProduction()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            Environments.Production,
            StringComparison.OrdinalIgnoreCase);
    }
}
