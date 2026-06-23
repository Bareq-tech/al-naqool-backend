using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace BareqAlNaqool.Infrastructure.Hosting;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddBareqHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        return services;
    }

    public static WebApplication MapBareqHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });

        return app;
    }
}
