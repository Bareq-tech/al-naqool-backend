using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminConversationsService(AppDbContext db) : IAdminCrudService<AdminConversationDto, AdminConversationCreateDto, AdminConversationUpdateDto>
{
    public async Task<IReadOnlyList<AdminConversationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Conversations.AsNoTracking().Include(x => x.Participants).OrderByDescending(x => x.LastMessageAt).ToListAsync(cancellationToken);
        var result = new List<AdminConversationDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminConversationDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.Conversations.AsNoTracking().Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminConversationDto> CreateAsync(AdminConversationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Conversation
        {
            IsGroup = dto.IsGroup,
            TypeKey = dto.Type,
            LastMessageAt = DateTime.UtcNow
        };
        db.Conversations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        await SyncParticipantsAsync(entity.Id, dto.ParticipantUserIds, cancellationToken);
        entity = await db.Conversations.Include(x => x.Participants).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminConversationDto?> UpdateAsync(string id, AdminConversationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Conversations.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.IsGroup = dto.IsGroup;
        entity.TypeKey = dto.Type;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        await SyncParticipantsAsync(entity.Id, dto.ParticipantUserIds, cancellationToken);
        entity = await db.Conversations.Include(x => x.Participants).FirstAsync(x => x.Id == entity.Id, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Conversations.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var participants = await db.ConversationParticipants.Where(x => x.ConversationId == entity.Id).ToListAsync(cancellationToken);
        db.ConversationParticipants.RemoveRange(participants);
        var messages = await db.ChatMessages.Where(x => x.ConversationId == entity.Id).ToListAsync(cancellationToken);
        db.ChatMessages.RemoveRange(messages);
        db.Conversations.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminConversationDto> MapAsync(Conversation item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.Conversation, item.Id, cancellationToken);
        var participantIds = item.Participants.Select(x => IdFormatter.ToStringId(x.UserId)).ToList();
        return new AdminConversationDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "lastMessage"),
            AdminTranslationHelper.Get(ar, "lastMessage"),
            item.IsGroup,
            item.TypeKey,
            item.LastMessageAt,
            participantIds);
    }

    private Task SaveTranslationsAsync(int id, AdminConversationUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminConversationCreateDto(dto.NameEn, dto.NameAr, dto.IsGroup, dto.Type, dto.ParticipantUserIds), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminConversationCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.Conversation,
            id,
            new { name = dto.NameEn, lastMessage = string.Empty, type = dto.Type },
            new { name = dto.NameAr, lastMessage = string.Empty, type = dto.Type },
            cancellationToken);

    private async Task SyncParticipantsAsync(int conversationId, IReadOnlyList<string> participantUserIds, CancellationToken cancellationToken)
    {
        var desiredUserIds = participantUserIds.Select(IdFormatter.ParseId).ToHashSet();
        var existing = await db.ConversationParticipants.Where(x => x.ConversationId == conversationId).ToListAsync(cancellationToken);
        db.ConversationParticipants.RemoveRange(existing.Where(x => !desiredUserIds.Contains(x.UserId)));
        var existingUserIds = existing.Select(x => x.UserId).ToHashSet();
        foreach (var userId in desiredUserIds.Where(x => !existingUserIds.Contains(x)))
        {
            db.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = userId,
                UnreadCount = 0
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
