using System.Text.Json;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class AppDataHelper(AppDbContext db, ILanguageContext languageContext)
{
    public string Lang => languageContext.Language;

    public async Task<List<string>> GetAppSettingListAsync(string key, CancellationToken cancellationToken)
    {
        var setting = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            return [];
        }

        var root = JsonDocument.Parse(setting.ValueJson).RootElement;
        if (root.TryGetProperty(Lang, out var values))
        {
            return values.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList();
        }

        return [];
    }
}
