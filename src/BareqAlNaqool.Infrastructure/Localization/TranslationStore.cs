using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Localization;

public static class TranslationStore
{
    public static async Task SaveAsync(
        Persistence.AppDbContext db,
        string entityType,
        int entityId,
        string language,
        object data,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data);
        var existing = await db.EntityTranslations
            .FirstOrDefaultAsync(
                x => x.EntityType == entityType && x.EntityId == entityId && x.Language == language,
                cancellationToken);

        if (existing is null)
        {
            db.EntityTranslations.Add(new Domain.Entities.EntityTranslation
            {
                EntityType = entityType,
                EntityId = entityId,
                Language = language,
                DataJson = json
            });
        }
        else
        {
            existing.DataJson = json;
        }
    }

    public static async Task<Dictionary<string, JsonElement>> GetMapAsync(
        Persistence.AppDbContext db,
        string entityType,
        int entityId,
        string language,
        CancellationToken cancellationToken = default)
    {
        var translation = await db.EntityTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EntityType == entityType && x.EntityId == entityId && x.Language == language,
                cancellationToken);

        if (translation is null)
        {
            translation = await db.EntityTranslations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.EntityType == entityType && x.EntityId == entityId && x.Language == "en",
                    cancellationToken);
        }

        if (translation is null)
        {
            return new Dictionary<string, JsonElement>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(translation.DataJson)
               ?? new Dictionary<string, JsonElement>();
    }

    public static string GetString(Dictionary<string, JsonElement> map, string key, string fallback = "")
    {
        if (map.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? fallback;
        }

        return fallback;
    }

    public static int GetInt(Dictionary<string, JsonElement> map, string key, int fallback = 0)
    {
        if (map.TryGetValue(key, out var value) && value.TryGetInt32(out var result))
        {
            return result;
        }

        return fallback;
    }

    public static bool GetBool(Dictionary<string, JsonElement> map, string key, bool fallback = false)
    {
        if (map.TryGetValue(key, out var value) &&
            (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            return value.GetBoolean();
        }

        return fallback;
    }
}

public static class TimeAgoFormatter
{
    public static string Format(DateTime dateTime, string language)
    {
        var now = DateTime.UtcNow;
        var diff = now - dateTime.ToUniversalTime();

        if (diff.TotalMinutes < 1)
        {
            return language == "ar" ? "الآن" : "Just now";
        }

        if (diff.TotalHours < 1)
        {
            var minutes = (int)diff.TotalMinutes;
            return language == "ar"
                ? $"منذ {minutes} دقيقة"
                : $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }

        if (diff.TotalHours < 24)
        {
            var hours = (int)diff.TotalHours;
            return language == "ar"
                ? $"منذ {hours} ساعة"
                : $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }

        if (diff.TotalDays < 2)
        {
            return language == "ar" ? "أمس" : "Yesterday";
        }

        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return language == "ar"
                ? $"منذ {days} أيام"
                : $"{days} days ago";
        }

        if (diff.TotalDays < 14)
        {
            return language == "ar" ? "منذ أسبوع" : "1 week ago";
        }

        return dateTime.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
    }

    public static string FormatTime(DateTime dateTime, string language)
    {
        if (dateTime.Date == DateTime.UtcNow.Date)
        {
            return dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        if (dateTime.Date == DateTime.UtcNow.Date.AddDays(-1))
        {
            return language == "ar" ? "أمس" : "Yesterday";
        }

        if (dateTime > DateTime.UtcNow.AddDays(-7))
        {
            return dateTime.ToString("ddd", CultureInfo.InvariantCulture);
        }

        return dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }
}

public static class IdFormatter
{
    public static string ToStringId(int id) => id.ToString(CultureInfo.InvariantCulture);

    public static int ParseId(string id) => int.Parse(id, CultureInfo.InvariantCulture);
}
