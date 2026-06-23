using System.Text.Json;
using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminSettingsService(AppDbContext db) : IAdminSettingsService
{
    private static readonly string[] KnownKeys =
    [
        AppSettingKeys.NewsCategories,
        AppSettingKeys.MessageFilters,
        AppSettingKeys.GalleryTypes,
        AppSettingKeys.DocumentCategories,
        AppSettingKeys.DirectoryBranches,
        AppSettingKeys.TermsAndConditions,
        AppSettingKeys.GuestPermissions,
        AppSettingKeys.AppBranding,
        AppSettingKeys.QuickAccessTiles
    ];

    public async Task<AdminAppSettingDto?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsKnownKey(key))
        {
            return null;
        }

        var setting = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        return setting is null ? null : new AdminAppSettingDto(setting.Key, setting.ValueJson);
    }

    public async Task<AdminAppSettingDto> UpdateSettingAsync(string key, AdminAppSettingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (!IsKnownKey(key))
        {
            throw new ArgumentException($"Unknown setting key: {key}", nameof(key));
        }

        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        setting.ValueJson = dto.ValueJson;
        await db.SaveChangesAsync(cancellationToken);
        return new AdminAppSettingDto(setting.Key, setting.ValueJson);
    }

    public async Task<AdminLocalizedListSettingDto> GetLocalizedListAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return new AdminLocalizedListSettingDto(
            key,
            ParseStringList(root, "en"),
            ParseStringList(root, "ar"));
    }

    public async Task<AdminLocalizedListSettingDto> UpdateLocalizedListAsync(string key, AdminLocalizedListSettingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(key, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new { en = dto.En, ar = dto.Ar });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminLocalizedListSettingDto(key, dto.En, dto.Ar);
    }

    public async Task<AdminTermsDto> GetTermsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.TermsAndConditions, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return new AdminTermsDto(
            root.TryGetProperty("en", out var en) ? en.GetString() ?? string.Empty : string.Empty,
            root.TryGetProperty("ar", out var ar) ? ar.GetString() ?? string.Empty : string.Empty);
    }

    public async Task<AdminTermsDto> UpdateTermsAsync(AdminTermsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.TermsAndConditions, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new { en = dto.En, ar = dto.Ar });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminTermsDto(dto.En, dto.Ar);
    }

    public async Task<AdminGuestPermissionsDto> GetGuestPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.GuestPermissions, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return ParseGuestPermissions(root);
    }

    public async Task<AdminGuestPermissionsDto> UpdateGuestPermissionsAsync(AdminGuestPermissionsUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.GuestPermissions, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new
        {
            news = dto.News,
            events = dto.Events,
            gallery = dto.Gallery,
            documents = dto.Documents,
            council = dto.Council,
            messages = dto.Messages,
            directory = dto.Directory,
            familyTree = dto.FamilyTree
        });
        await db.SaveChangesAsync(cancellationToken);
        return MapGuestPermissions(dto);
    }

    public async Task<AdminBrandingDto> GetBrandingAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.AppBranding, cancellationToken);
        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        return ParseBranding(root);
    }

    public async Task<AdminBrandingDto> UpdateBrandingAsync(AdminBrandingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.AppBranding, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(new
        {
            appNameEn = dto.AppNameEn,
            appNameAr = dto.AppNameAr,
            logoUrl = dto.LogoUrl,
            primaryColorHex = dto.PrimaryColorHex
        });
        await db.SaveChangesAsync(cancellationToken);
        return new AdminBrandingDto(dto.AppNameEn, dto.AppNameAr, dto.LogoUrl, dto.PrimaryColorHex);
    }

    public async Task<IReadOnlyList<AdminQuickAccessTileDto>> GetQuickAccessTilesAsync(CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.QuickAccessTiles, cancellationToken);
        return ParseQuickAccessTiles(setting.ValueJson);
    }

    public async Task<IReadOnlyList<AdminQuickAccessTileDto>> UpdateQuickAccessTilesAsync(AdminQuickAccessTilesUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var setting = await GetOrCreateSettingAsync(AppSettingKeys.QuickAccessTiles, cancellationToken);
        setting.ValueJson = JsonSerializer.Serialize(dto.Tiles.Select(x => new
        {
            key = x.Key,
            labelEn = x.LabelEn,
            labelAr = x.LabelAr,
            iconName = x.IconName,
            route = x.Route,
            sortOrder = x.SortOrder
        }));
        await db.SaveChangesAsync(cancellationToken);
        return dto.Tiles;
    }

    private async Task<AppSetting> GetOrCreateSettingAsync(string key, CancellationToken cancellationToken)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is not null)
        {
            return setting;
        }

        setting = new AppSetting { Key = key, ValueJson = GetDefaultValueJson(key) };
        db.AppSettings.Add(setting);
        await db.SaveChangesAsync(cancellationToken);
        return setting;
    }

    private static bool IsKnownKey(string key) => KnownKeys.Contains(key);

    private static string GetDefaultValueJson(string key) => key switch
    {
        AppSettingKeys.TermsAndConditions => JsonSerializer.Serialize(new { en = string.Empty, ar = string.Empty }),
        AppSettingKeys.GuestPermissions => JsonSerializer.Serialize(new
        {
            news = true,
            events = true,
            gallery = true,
            documents = false,
            council = false,
            messages = false,
            directory = false,
            familyTree = true
        }),
        AppSettingKeys.AppBranding => JsonSerializer.Serialize(new
        {
            appNameEn = string.Empty,
            appNameAr = string.Empty,
            logoUrl = string.Empty,
            primaryColorHex = string.Empty
        }),
        AppSettingKeys.QuickAccessTiles => "[]",
        _ => JsonSerializer.Serialize(new { en = Array.Empty<string>(), ar = Array.Empty<string>() })
    };

    private static IReadOnlyList<string> ParseStringList(JsonElement root, string language)
    {
        if (!root.TryGetProperty(language, out var values) || values.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return values.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
    }

    private static AdminGuestPermissionsDto ParseGuestPermissions(JsonElement root) =>
        new(
            GetBool(root, "news"),
            GetBool(root, "events"),
            GetBool(root, "gallery"),
            GetBool(root, "documents"),
            GetBool(root, "council"),
            GetBool(root, "messages"),
            GetBool(root, "directory"),
            GetBool(root, "familyTree"));

    private static AdminGuestPermissionsDto MapGuestPermissions(AdminGuestPermissionsUpdateDto dto) =>
        new(dto.News, dto.Events, dto.Gallery, dto.Documents, dto.Council, dto.Messages, dto.Directory, dto.FamilyTree);

    private static AdminBrandingDto ParseBranding(JsonElement root) =>
        new(
            GetString(root, "appNameEn"),
            GetString(root, "appNameAr"),
            GetString(root, "logoUrl"),
            GetString(root, "primaryColorHex"));

    private static IReadOnlyList<AdminQuickAccessTileDto> ParseQuickAccessTiles(string valueJson)
    {
        var root = JsonDocument.Parse(valueJson).RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return root.EnumerateArray().Select(x => new AdminQuickAccessTileDto(
            GetString(x, "key"),
            GetString(x, "labelEn"),
            GetString(x, "labelAr"),
            GetString(x, "iconName"),
            GetString(x, "route"),
            x.TryGetProperty("sortOrder", out var sort) && sort.TryGetInt32(out var order) ? order : 0)).ToList();
    }

    private static string GetString(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    private static bool GetBool(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
}
