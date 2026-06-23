using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EntityTranslation> EntityTranslations => Set<EntityTranslation>();
    public DbSet<LandingSlide> LandingSlides => Set<LandingSlide>();
    public DbSet<HomeStat> HomeStats => Set<HomeStat>();
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    public DbSet<EventItem> Events => Set<EventItem>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<FamilyBranch> FamilyBranches => Set<FamilyBranch>();
    public DbSet<BranchMember> BranchMembers => Set<BranchMember>();
    public DbSet<TreeMember> TreeMembers => Set<TreeMember>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<GalleryPhoto> GalleryPhotos => Set<GalleryPhoto>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<CouncilModule> CouncilModules => Set<CouncilModule>();
    public DbSet<CouncilMeeting> CouncilMeetings => Set<CouncilMeeting>();
    public DbSet<CouncilListItem> CouncilListItems => Set<CouncilListItem>();
    public DbSet<DirectoryMember> DirectoryMembers => Set<DirectoryMember>();
    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EntityTranslation>()
            .HasIndex(x => new { x.EntityType, x.EntityId, x.Language })
            .IsUnique();

        builder.Entity<EventRegistration>()
            .HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique();

        builder.Entity<UserNotification>()
            .HasIndex(x => new { x.NotificationId, x.UserId })
            .IsUnique();

        builder.Entity<ConversationParticipant>()
            .HasIndex(x => new { x.ConversationId, x.UserId })
            .IsUnique();

        builder.Entity<AppSetting>()
            .HasIndex(x => x.Key)
            .IsUnique();
    }
}
