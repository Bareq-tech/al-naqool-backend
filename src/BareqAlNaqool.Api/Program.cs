using BareqAlNaqool.Infrastructure;
using BareqAlNaqool.Infrastructure.Hosting;
using BareqAlNaqool.Infrastructure.OpenApi;
using BareqAlNaqool.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureRailwayPort();
builder.ValidateProductionSettings();

builder.Services.AddControllers();
builder.Services.AddBareqSwagger("Bareq Al Naqool Mobile API");
builder.Services.AddBareqCors(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBareqHealthChecks();

var app = builder.Build();

app.Logger.LogInformation("Database target: {DatabaseTarget}", DatabaseConnection.Describe(app.Configuration));

if (app.Environment.IsProduction())
{
    await DatabaseConnection.VerifyConnectivityAsync(
        DatabaseConnection.Resolve(app.Configuration),
        app.Logger);
}

await DatabaseStartup.ApplyConfiguredStartupAsync(
    app.Services,
    app.Configuration,
    app.Logger,
    app.Environment);

var storageAdminBaseUrl = app.Configuration["Storage:AdminFilesBaseUrl"];
if (app.Environment.IsProduction() && string.IsNullOrWhiteSpace(storageAdminBaseUrl))
{
    app.Logger.LogWarning(
        "Storage:AdminFilesBaseUrl is not configured. Document downloads will fail unless files exist on this service. Set Storage__AdminFilesBaseUrl to the admin API public URL.");
}

app.MapBareqHealthChecks();
app.UseBareqCors();
app.UseBareqSwagger("Bareq Al Naqool Mobile API");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
