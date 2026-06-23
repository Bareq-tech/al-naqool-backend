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

public class AdminContactService(AppDbContext db) : IAdminContactService
{
    public async Task<IReadOnlyList<AdminContactSubmissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.ContactSubmissions.AsNoTracking().OrderByDescending(x => x.SubmittedAt).ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<AdminContactSubmissionDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.ContactSubmissions.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.ContactSubmissions.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AdminContactSubmissionDto Map(ContactSubmission item) =>
        new(
            IdFormatter.ToStringId(item.Id),
            item.Name,
            item.Email,
            item.Subject,
            item.Message,
            item.SubmittedAt,
            item.IsRead);
}
