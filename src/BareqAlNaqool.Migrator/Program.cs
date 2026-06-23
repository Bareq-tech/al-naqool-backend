using BareqAlNaqool.Infrastructure;
using BareqAlNaqool.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var seed = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
using var scope = host.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrator");

logger.LogInformation("Applying EF Core migrations.");
await DataSeeder.MigrateAsync(scope.ServiceProvider);

if (seed)
{
    logger.LogInformation("Seeding database.");
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

logger.LogInformation("Database migration completed.");
