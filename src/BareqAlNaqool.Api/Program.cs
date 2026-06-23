using BareqAlNaqool.Infrastructure;
using BareqAlNaqool.Infrastructure.Hosting;
using BareqAlNaqool.Infrastructure.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureRailwayPort();
builder.ValidateProductionSettings();

builder.Services.AddControllers();
builder.Services.AddBareqSwagger("Bareq Al Naqool Mobile API");
builder.Services.AddBareqCors(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBareqHealthChecks();

var app = builder.Build();

await DatabaseStartup.ApplyConfiguredStartupAsync(
    app.Services,
    app.Configuration,
    app.Logger);

app.MapBareqHealthChecks();
app.UseBareqCors();
app.UseBareqSwagger("Bareq Al Naqool Mobile API");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
