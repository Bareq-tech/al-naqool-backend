using BareqAlNaqool.Application.DTOs;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services;

public class MessagesService(AppDbContext db, AppDataHelper helper) : IMessagesService
{
    public async Task<IReadOnlyList<string>> GetFilterTypesAsync(CancellationToken cancellationToken = default)
        => await helper.GetAppSettingListAsync(AppSettingKeys.MessageFilters, cancellationToken);

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string? type, int? userId, CancellationToken cancellationToken = default)
    {
        var conversations = await db.Conversations.AsNoTracking().OrderByDescending(x => x.LastMessageAt).ToListAsync(cancellationToken);
        var result = new List<ConversationDto>();
        foreach (var conversation in conversations)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.Conversation, conversation.Id, helper.Lang, cancellationToken);
            var conversationType = TranslationStore.GetString(map, "type");
            if (type is not null && !string.Equals(type, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(conversationType, type, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var unread = 0;
            if (userId.HasValue)
            {
                unread = await db.ConversationParticipants.AsNoTracking()
                    .Where(x => x.ConversationId == conversation.Id && x.UserId == userId.Value)
                    .Select(x => x.UnreadCount)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            result.Add(new ConversationDto(
                IdFormatter.ToStringId(conversation.Id),
                TranslationStore.GetString(map, "name"),
                TranslationStore.GetString(map, "lastMessage"),
                TranslationStore.GetString(map, "time"),
                unread > 0 ? unread : TranslationStore.GetInt(map, "unreadCount"),
                conversation.IsGroup,
                conversationType));
        }

        return result;
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(string id, int? userId, CancellationToken cancellationToken = default)
    {
        var conversation = await db.Conversations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (conversation is null)
        {
            return null;
        }

        var map = await TranslationStore.GetMapAsync(db, EntityTypes.Conversation, conversation.Id, helper.Lang, cancellationToken);
        var unread = 0;
        if (userId.HasValue)
        {
            unread = await db.ConversationParticipants.AsNoTracking()
                .Where(x => x.ConversationId == conversation.Id && x.UserId == userId.Value)
                .Select(x => x.UnreadCount)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ConversationDto(
            IdFormatter.ToStringId(conversation.Id),
            TranslationStore.GetString(map, "name"),
            TranslationStore.GetString(map, "lastMessage"),
            TranslationStore.GetString(map, "time"),
            unread,
            conversation.IsGroup,
            TranslationStore.GetString(map, "type"));
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetChatMessagesAsync(string conversationId, int? userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var messages = await db.ChatMessages.AsNoTracking()
            .Where(x => x.ConversationId == parsedId && !x.IsHidden)
            .OrderBy(x => x.SentAt)
            .ToListAsync(cancellationToken);
        var result = new List<ChatMessageDto>();
        foreach (var message in messages)
        {
            var map = await TranslationStore.GetMapAsync(db, EntityTypes.ChatMessage, message.Id, helper.Lang, cancellationToken);
            var senderName = TranslationStore.GetString(map, "senderName");
            if (userId.HasValue && message.SenderUserId == userId.Value)
            {
                senderName = helper.Lang == "ar" ? "أنت" : "You";
            }

            result.Add(new ChatMessageDto(
                IdFormatter.ToStringId(message.Id),
                IdFormatter.ToStringId(message.ConversationId),
                senderName,
                TranslationStore.GetString(map, "content"),
                TimeAgoFormatter.FormatTime(message.SentAt, helper.Lang),
                userId.HasValue && message.SenderUserId == userId.Value,
                message.IsAnnouncement));
        }

        return result;
    }

    public async Task SendMessageAsync(string conversationId, string content, int userId, CancellationToken cancellationToken = default)
    {
        var parsedId = IdFormatter.ParseId(conversationId);
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId, cancellationToken);
        var message = new ChatMessage
        {
            ConversationId = parsedId,
            SenderUserId = userId,
            SenderNameKey = user.FullName,
            SentAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(message);
        var conversation = await db.Conversations.FirstAsync(x => x.Id == parsedId, cancellationToken);
        conversation.LastMessageAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var enData = new { content, senderName = "You", time = TimeAgoFormatter.FormatTime(message.SentAt, "en"), isMe = true };
        var arData = new { content, senderName = "أنت", time = TimeAgoFormatter.FormatTime(message.SentAt, "ar"), isMe = true };
        await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, message.Id, "en", enData, cancellationToken);
        await TranslationStore.SaveAsync(db, EntityTypes.ChatMessage, message.Id, "ar", arData, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
