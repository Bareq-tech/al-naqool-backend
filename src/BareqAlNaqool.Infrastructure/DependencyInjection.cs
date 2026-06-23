using System.Text;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Options;
using BareqAlNaqool.Infrastructure.Persistence;
using BareqAlNaqool.Infrastructure.Security;
using BareqAlNaqool.Infrastructure.Services;
using BareqAlNaqool.Infrastructure.Services.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BareqAlNaqool.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "DefaultConnection")
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(DatabaseConnection.Resolve(configuration, connectionStringName)));

        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ILanguageContext, Localization.LanguageContext>();
        services.AddScoped<AppDataHelper>();
        services.AddScoped<JwtTokenService>();

        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IFamilyTreeService, FamilyTreeService>();
        services.AddScoped<IMessagesService, MessagesService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IDocumentsService, DocumentsService>();
        services.AddScoped<ICouncilService, CouncilService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDirectoryService, DirectoryService>();
        services.AddScoped<INotificationsService, NotificationsService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IAdminCrudService<AdminNewsDto, AdminNewsCreateDto, AdminNewsUpdateDto>, AdminNewsService>();
        services.AddScoped<IAdminCrudService<AdminEventDto, AdminEventCreateDto, AdminEventUpdateDto>, AdminEventsService>();
        services.AddScoped<IAdminCrudService<AdminBranchDto, AdminBranchCreateDto, AdminBranchUpdateDto>, AdminBranchesService>();
        services.AddScoped<IAdminCrudService<AdminAlbumDto, AdminAlbumCreateDto, AdminAlbumUpdateDto>, AdminAlbumsService>();
        services.AddScoped<IAdminCrudService<AdminDocumentDto, AdminDocumentCreateDto, AdminDocumentUpdateDto>, AdminDocumentsService>();
        services.AddScoped<IAdminCrudService<AdminNotificationDto, AdminNotificationCreateDto, AdminNotificationUpdateDto>, AdminNotificationsService>();
        services.AddScoped<IAdminCrudService<AdminCouncilItemDto, AdminCouncilItemCreateDto, AdminCouncilItemUpdateDto>, AdminCouncilItemsService>();
        services.AddScoped<IAdminCrudService<AdminDirectoryMemberDto, AdminDirectoryMemberCreateDto, AdminDirectoryMemberUpdateDto>, AdminDirectoryService>();
        services.AddScoped<IAdminCrudService<AdminUserDto, AdminUserCreateDto, AdminUserUpdateDto>, AdminUsersService>();

        return services;
    }
}
