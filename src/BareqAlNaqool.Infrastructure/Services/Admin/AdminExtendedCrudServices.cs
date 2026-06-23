using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminBranchMembersService(AppDbContext db) : IAdminCrudService<AdminBranchMemberDto, AdminBranchMemberCreateDto, AdminBranchMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminBranchMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.BranchMembers.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AdminBranchMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminBranchMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.BranchMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminBranchMemberDto> CreateAsync(AdminBranchMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new BranchMember
        {
            BranchId = IdFormatter.ParseId(dto.BranchId),
            ImageUrl = dto.ImageUrl
        };
        db.BranchMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminBranchMemberDto?> UpdateAsync(string id, AdminBranchMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.BranchMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.BranchId = IdFormatter.ParseId(dto.BranchId);
        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.BranchMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.BranchMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminBranchMemberDto> MapAsync(BranchMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.BranchMember, item.Id, cancellationToken);
        return new AdminBranchMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "role"),
            AdminTranslationHelper.Get(ar, "role"),
            IdFormatter.ToStringId(item.BranchId),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminBranchMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminBranchMemberCreateDto(dto.NameEn, dto.NameAr, dto.RoleEn, dto.RoleAr, dto.BranchId, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminBranchMemberCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.BranchMember,
            id,
            new { name = dto.NameEn, role = dto.RoleEn },
            new { name = dto.NameAr, role = dto.RoleAr },
            cancellationToken);
}

public class AdminTreeMembersService(AppDbContext db) : IAdminCrudService<AdminTreeMemberDto, AdminTreeMemberCreateDto, AdminTreeMemberUpdateDto>
{
    public async Task<IReadOnlyList<AdminTreeMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.TreeMembers.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminTreeMemberDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminTreeMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.TreeMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminTreeMemberDto> CreateAsync(AdminTreeMemberCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new TreeMember
        {
            Generation = dto.Generation,
            IsFounder = dto.IsFounder,
            ImageUrl = dto.ImageUrl,
            SortOrder = dto.SortOrder
        };
        db.TreeMembers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminTreeMemberDto?> UpdateAsync(string id, AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Generation = dto.Generation;
        entity.IsFounder = dto.IsFounder;
        entity.ImageUrl = dto.ImageUrl;
        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.TreeMembers.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.TreeMembers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminTreeMemberDto> MapAsync(TreeMember item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.TreeMember, item.Id, cancellationToken);
        return new AdminTreeMemberDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.Generation,
            item.IsFounder,
            item.ImageUrl,
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminTreeMemberUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminTreeMemberCreateDto(dto.NameEn, dto.NameAr, dto.SubtitleEn, dto.SubtitleAr, dto.Generation, dto.IsFounder, dto.ImageUrl, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminTreeMemberCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.TreeMember,
            id,
            new { name = dto.NameEn, subtitle = dto.SubtitleEn },
            new { name = dto.NameAr, subtitle = dto.SubtitleAr },
            cancellationToken);
}

public class AdminLandingSlidesService(AppDbContext db) : IAdminCrudService<AdminLandingSlideDto, AdminLandingSlideCreateDto, AdminLandingSlideUpdateDto>
{
    public async Task<IReadOnlyList<AdminLandingSlideDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.LandingSlides.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminLandingSlideDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminLandingSlideDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.LandingSlides.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminLandingSlideDto> CreateAsync(AdminLandingSlideCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new LandingSlide { SortOrder = dto.SortOrder };
        db.LandingSlides.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminLandingSlideDto?> UpdateAsync(string id, AdminLandingSlideUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.LandingSlides.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.LandingSlides.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.LandingSlides.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminLandingSlideDto> MapAsync(LandingSlide item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.LandingSlide, item.Id, cancellationToken);
        return new AdminLandingSlideDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "titleLine1"),
            AdminTranslationHelper.Get(ar, "titleLine1"),
            AdminTranslationHelper.Get(en, "titleLine2"),
            AdminTranslationHelper.Get(ar, "titleLine2"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminLandingSlideUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminLandingSlideCreateDto(dto.TitleLine1En, dto.TitleLine1Ar, dto.TitleLine2En, dto.TitleLine2Ar, dto.SubtitleEn, dto.SubtitleAr, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminLandingSlideCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.LandingSlide,
            id,
            new { titleLine1 = dto.TitleLine1En, titleLine2 = dto.TitleLine2En, subtitle = dto.SubtitleEn },
            new { titleLine1 = dto.TitleLine1Ar, titleLine2 = dto.TitleLine2Ar, subtitle = dto.SubtitleAr },
            cancellationToken);
}

public class AdminGalleryPhotosService(AppDbContext db) : IAdminCrudService<AdminGalleryPhotoDto, AdminGalleryPhotoCreateDto, AdminGalleryPhotoUpdateDto>
{
    public async Task<IReadOnlyList<AdminGalleryPhotoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.GalleryPhotos.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<AdminGalleryPhotoDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminGalleryPhotoDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.GalleryPhotos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminGalleryPhotoDto> CreateAsync(AdminGalleryPhotoCreateDto dto, CancellationToken cancellationToken = default)
    {
        var albumId = IdFormatter.ParseId(dto.AlbumId);
        var album = await db.Albums.FirstAsync(x => x.Id == albumId, cancellationToken);
        var entity = new GalleryPhoto
        {
            AlbumId = albumId,
            ImageUrl = dto.ImageUrl
        };
        db.GalleryPhotos.Add(entity);
        album.PhotoCount++;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminGalleryPhotoDto?> UpdateAsync(string id, AdminGalleryPhotoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.GalleryPhotos.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ImageUrl = dto.ImageUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.GalleryPhotos.Include(x => x.Album).FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Album.PhotoCount = Math.Max(0, entity.Album.PhotoCount - 1);
        db.GalleryPhotos.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminGalleryPhotoDto> MapAsync(GalleryPhoto item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.GalleryPhoto, item.Id, cancellationToken);
        return new AdminGalleryPhotoDto(
            IdFormatter.ToStringId(item.Id),
            IdFormatter.ToStringId(item.AlbumId),
            AdminTranslationHelper.Get(en, "caption"),
            AdminTranslationHelper.Get(ar, "caption"),
            item.ImageUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminGalleryPhotoUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminGalleryPhotoCreateDto(string.Empty, dto.CaptionEn, dto.CaptionAr, dto.ImageUrl), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminGalleryPhotoCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.GalleryPhoto,
            id,
            new { caption = dto.CaptionEn },
            new { caption = dto.CaptionAr },
            cancellationToken);
}

public class AdminCouncilModulesService(AppDbContext db) : IAdminCrudService<AdminCouncilModuleDto, AdminCouncilModuleCreateDto, AdminCouncilModuleUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilModuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilModules.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var result = new List<AdminCouncilModuleDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilModuleDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilModules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilModuleDto> CreateAsync(AdminCouncilModuleCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilModule
        {
            ModuleKey = dto.ModuleKey,
            IconName = dto.IconName,
            SortOrder = dto.SortOrder
        };
        db.CouncilModules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilModuleDto?> UpdateAsync(string id, AdminCouncilModuleUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilModules.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleKey = dto.ModuleKey;
        entity.IconName = dto.IconName;
        entity.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilModules.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilModules.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilModuleDto> MapAsync(CouncilModule item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilModule, item.Id, cancellationToken);
        return new AdminCouncilModuleDto(
            IdFormatter.ToStringId(item.Id),
            item.ModuleKey,
            item.IconName,
            AdminTranslationHelper.Get(en, "label"),
            AdminTranslationHelper.Get(ar, "label"),
            AdminTranslationHelper.Get(en, "subtitle"),
            AdminTranslationHelper.Get(ar, "subtitle"),
            item.SortOrder);
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilModuleUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilModuleCreateDto(dto.ModuleKey, dto.IconName, dto.LabelEn, dto.LabelAr, dto.SubtitleEn, dto.SubtitleAr, dto.SortOrder), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilModuleCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilModule,
            id,
            new { id = dto.ModuleKey, label = dto.LabelEn, subtitle = dto.SubtitleEn },
            new { id = dto.ModuleKey, label = dto.LabelAr, subtitle = dto.SubtitleAr },
            cancellationToken);
}

public class AdminCouncilMeetingsService(AppDbContext db) : IAdminCrudService<AdminCouncilMeetingDto, AdminCouncilMeetingCreateDto, AdminCouncilMeetingUpdateDto>
{
    public async Task<IReadOnlyList<AdminCouncilMeetingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.CouncilMeetings.AsNoTracking().OrderByDescending(x => x.MeetingDate).ToListAsync(cancellationToken);
        var result = new List<AdminCouncilMeetingDto>();
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<AdminCouncilMeetingDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await db.CouncilMeetings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<AdminCouncilMeetingDto> CreateAsync(AdminCouncilMeetingCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new CouncilMeeting
        {
            MeetingDate = dto.MeetingDate,
            TimeValue = dto.Time,
            Decisions = dto.Decisions,
            Tasks = dto.Tasks,
            Attachments = dto.Attachments,
            IsLatest = dto.IsLatest,
            MinutesFileUrl = dto.MinutesFileUrl
        };
        db.CouncilMeetings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<AdminCouncilMeetingDto?> UpdateAsync(string id, AdminCouncilMeetingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilMeetings.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.MeetingDate = dto.MeetingDate;
        entity.TimeValue = dto.Time;
        entity.Decisions = dto.Decisions;
        entity.Tasks = dto.Tasks;
        entity.Attachments = dto.Attachments;
        entity.IsLatest = dto.IsLatest;
        entity.MinutesFileUrl = dto.MinutesFileUrl;
        await db.SaveChangesAsync(cancellationToken);
        await SaveTranslationsAsync(entity.Id, dto, cancellationToken);
        return await MapAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CouncilMeetings.FirstOrDefaultAsync(x => x.Id == IdFormatter.ParseId(id), cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.CouncilMeetings.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AdminCouncilMeetingDto> MapAsync(CouncilMeeting item, CancellationToken cancellationToken)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.CouncilMeeting, item.Id, cancellationToken);
        return new AdminCouncilMeetingDto(
            IdFormatter.ToStringId(item.Id),
            AdminTranslationHelper.Get(en, "title"),
            AdminTranslationHelper.Get(ar, "title"),
            AdminTranslationHelper.Get(en, "date"),
            AdminTranslationHelper.Get(ar, "date"),
            item.TimeValue,
            AdminTranslationHelper.Get(en, "location"),
            AdminTranslationHelper.Get(ar, "location"),
            item.Decisions,
            item.Tasks,
            item.Attachments,
            item.IsLatest,
            item.MinutesFileUrl);
    }

    private Task SaveTranslationsAsync(int id, AdminCouncilMeetingUpdateDto dto, CancellationToken cancellationToken) =>
        SaveTranslationsAsync(id, new AdminCouncilMeetingCreateDto(dto.TitleEn, dto.TitleAr, dto.DateEn, dto.DateAr, dto.Time, dto.LocationEn, dto.LocationAr, dto.Decisions, dto.Tasks, dto.Attachments, dto.IsLatest, dto.MinutesFileUrl, dto.MeetingDate), cancellationToken);

    private Task SaveTranslationsAsync(int id, AdminCouncilMeetingCreateDto dto, CancellationToken cancellationToken) =>
        AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.CouncilMeeting,
            id,
            new { title = dto.TitleEn, date = dto.DateEn, time = dto.Time, location = dto.LocationEn },
            new { title = dto.TitleAr, date = dto.DateAr, time = dto.Time, location = dto.LocationAr },
            cancellationToken);
}

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
