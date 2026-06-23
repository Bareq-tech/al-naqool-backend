using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class DocumentsService(AppDbContext db, AppDataHelper helper) : IDocumentsService
{
    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.DocumentCategories, cancellationToken);

    public async Task<IReadOnlyList<DocumentItemDto>> GetDocumentsAsync(string? category, CancellationToken cancellationToken = default)
    {
        var docs = await db.Documents.AsNoTracking().OrderByDescending(x => x.DocumentDate).ToListAsync(cancellationToken);
        var result = new List<DocumentItemDto>();
        foreach (var doc in docs)
        {
            var dto = await MapDocumentAsync(doc, cancellationToken);
            if (category is not null && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.Category, category, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<DocumentItemDto?> GetDocumentByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return doc is null ? null : await MapDocumentAsync(doc, cancellationToken);
    }

    private async Task<DocumentItemDto> MapDocumentAsync(Document doc, CancellationToken cancellationToken)
    {
        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Document, doc.Id, helper.Lang, cancellationToken);
        return new DocumentItemDto(
            IdFormatter.ToStringId(doc.Id),
            TranslationStore.GetString(map, "title"),
            doc.FileSize,
            TranslationStore.GetString(map, "date"),
            TranslationStore.GetString(map, "category"),
            TranslationStore.GetString(map, "description"),
            doc.FileUrl);
    }
}
