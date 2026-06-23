using BareqAlNaqool.Infrastructure;
using BareqAlNaqool.Infrastructure.Hosting;
using BareqAlNaqool.Infrastructure.OpenApi;
using BareqAlNaqool.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureRailwayPort();

builder.Services.AddControllers();
builder.Services.AddBareqSwagger("Bareq Al Naqool Admin API");
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.MigrateAsync(scope.ServiceProvider);
}

app.UseBareqSwagger("Bareq Al Naqool Admin API");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
