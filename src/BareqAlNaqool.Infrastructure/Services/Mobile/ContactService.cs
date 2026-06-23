using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Persistence;

namespace BareqAlNaqool.Infrastructure.Services;

public class ContactService(AppDbContext db) : IContactService
{
    public async Task SubmitContactAsync(ContactRequestDto request, CancellationToken cancellationToken = default)
    {
        db.ContactSubmissions.Add(new ContactSubmission
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            SubmittedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
