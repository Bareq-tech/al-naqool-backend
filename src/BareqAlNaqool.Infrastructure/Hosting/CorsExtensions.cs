using BareqAlNaqool.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class CorsExtensions
{
    public static IServiceCollection AddBareqCors(this IServiceCollection services, IConfiguration configuration)
    {
        var cors = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        var origins = cors.GetOrigins();

        if (origins.Count == 0)
        {
            return services;
        }

        services.AddCors(options =>
        {
            options.AddPolicy(CorsSettings.PolicyName, policy =>
            {
                policy.WithOrigins(origins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static WebApplication UseBareqCors(this WebApplication app)
    {
        var cors = app.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        if (cors.GetOrigins().Count == 0)
        {
            return app;
        }

        app.UseCors(CorsSettings.PolicyName);
        return app;
    }
}
