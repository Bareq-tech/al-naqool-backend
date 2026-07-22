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

public class AdminMessagingService(AppDbContext db) : IAdminMessagingService
{
    public async Task<IReadOnlyList<AdminChatMessageDto>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var messages = await db.ChatMessages.AsNoTracking()
            .Where(x => x.ConversationId == parsedId)
            .OrderBy(x => x.SentAt)
            .ToListAsync(cancellationToken);

        var result = new List<AdminChatMessageDto>();
        foreach (var message in messages)
        {
            result.Add(await MapAsync(message, cancellationToken));
        }

        return result;
    }

    public async Task<AdminChatMessageDto> BroadcastAsync(AdminBroadcastMessageDto dto, int adminUserId, CancellationToken cancellationToken = default)
    {
        var conversationId = IdFormatter.ParseId(dto.ConversationId);
        var admin = await db.Users.AsNoTracking().FirstAsync(x => x.Id == adminUserId, cancellationToken);
        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderUserId = adminUserId,
            SenderNameKey = admin.FullName,
            IsAnnouncement = dto.IsAnnouncement,
            SentAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(message);
        var conversation = await db.Conversations.FirstAsync(x => x.Id == conversationId, cancellationToken);
        conversation.LastMessageAt = message.SentAt;
        await db.SaveChangesAsync(cancellationToken);

        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.ChatMessage,
            message.Id,
            new { content = dto.ContentEn, senderName = admin.FullName, time = TimeAgoFormatter.FormatTime(message.SentAt, "en") },
            new { content = dto.ContentAr, senderName = admin.FullName, time = TimeAgoFormatter.FormatTime(message.SentAt, "ar") },
            cancellationToken);

        await AdminTranslationHelper.MergeBilingualAsync(
            db,
            EntityTypes.Conversation,
            conversationId,
            new { lastMessage = dto.ContentEn },
            new { lastMessage = dto.ContentAr },
            cancellationToken);

        return await MapAsync(message, cancellationToken);
    }

    public async Task<bool> HideMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var message = await db.ChatMessages.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(messageId), cancellationToken);
        if (message is null)
        {
            return false;
        }

        message.IsHidden = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var message = await db.ChatMessages.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(messageId), cancellationToken);
        if (message is null)
        {
            return false;
        }

        db.ChatMessages.Remove(message);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminChatMessageDto> MapAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.ChatMessage, message.Id, cancellationToken);
        var senderNameEn = AdminTranslationHelper.Get(en, "senderName");
        var senderNameAr = AdminTranslationHelper.Get(ar, "senderName");
        if (string.IsNullOrEmpty(senderNameEn))
        {
            senderNameEn = message.SenderNameKey;
        }

        if (string.IsNullOrEmpty(senderNameAr))
        {
            senderNameAr = message.SenderNameKey;
        }

        return new AdminChatMessageDto(
            IdFormatter.ToStringId(message.Id),
            IdFormatter.ToStringId(message.ConversationId),
            senderNameEn,
            senderNameAr,
            AdminTranslationHelper.Get(en, "content"),
            AdminTranslationHelper.Get(ar, "content"),
            message.IsAnnouncement,
            message.IsHidden,
            message.SentAt,
            message.SenderUserId.HasValue ? IdFormatter.ToStringId(message.SenderUserId.Value) : null);
    }
}
