using System.Text.Json;
using BareqAlNaqool.Infrastructure.Persistence;

namespace BareqAlNaqool.Infrastructure.Localization;

public static class AdminTranslationHelper
{
    public static async Task SaveBilingualAsync(
        AppDbContext db,
        string entityType,
        int entityId,
        object enData,
        object arData,
        CancellationToken cancellationToken = default)
    {
        await TranslationStore.SaveAsync(db, entityType, entityId, "en", enData, cancellationToken);
        await TranslationStore.SaveAsync(db, entityType, entityId, "ar", arData, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task<(Dictionary<string, JsonElement> En, Dictionary<string, JsonElement> Ar)> GetBilingualAsync(
        AppDbContext db,
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var en = await TranslationStore.GetMapAsync(db, entityType, entityId, "en", cancellationToken);
        var ar = await TranslationStore.GetMapAsync(db, entityType, entityId, "ar", cancellationToken);
        return (en, ar);
    }

    public static string Get(Dictionary<string, JsonElement> map, string key)
        => TranslationStore.GetString(map, key);
}
