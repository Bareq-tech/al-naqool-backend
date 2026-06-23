using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class HostConfiguration
{
    public static void ConfigureRailwayPort(this WebApplicationBuilder builder)
    {
        var port = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrWhiteSpace(port))
        {
            return;
        }

        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }
}
