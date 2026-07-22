namespace BareqAlNaqool.Domain.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Member = "Member";
    public const string Guest = "Guest";
    public const string BranchHead = "BranchHead";
    public const string Secretary = "Secretary";
    public const string CouncilAdmin = "CouncilAdmin";
    public const string CouncilPresident = "CouncilPresident";
    public const string CommitteeOrganizer = "CommitteeOrganizer";

    public static readonly string[] All =
    [
        Admin, Member, Guest, BranchHead, Secretary,
        CouncilAdmin, CouncilPresident, CommitteeOrganizer
    ];
}

public static class AccountStatuses
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Rejected = "Rejected";
    public const string Suspended = "Suspended";
}

public static class PublishStatuses
{
    public const string Draft = "Draft";
    public const string Scheduled = "Scheduled";
    public const string Published = "Published";
}

public static class AccessLevels
{
    public const string Public = "Public";
    public const string Members = "Members";
    public const string Council = "Council";
    public const string Guests = "Guests";
}

public static class EntityTypes
{
    public const string LandingSlide = "LandingSlide";
    public const string NewsItem = "NewsItem";
    public const string EventItem = "EventItem";
    public const string FamilyBranch = "FamilyBranch";
    public const string BranchMember = "BranchMember";
    public const string TreeMember = "TreeMember";
    public const string Conversation = "Conversation";
    public const string ChatMessage = "ChatMessage";
    public const string Album = "Album";
    public const string GalleryPhoto = "GalleryPhoto";
    public const string Document = "Document";
    public const string CouncilModule = "CouncilModule";
    public const string CouncilMeeting = "CouncilMeeting";
    public const string CouncilListItem = "CouncilListItem";
    public const string DirectoryMember = "DirectoryMember";
    public const string NotificationItem = "NotificationItem";
    public const string President = "President";
}

public static class AppSettingKeys
{
    public const string NewsCategories = "newsCategories";
    public const string MessageFilters = "messageFilters";
    public const string GalleryTypes = "galleryTypes";
    public const string DocumentCategories = "documentCategories";
    public const string DirectoryBranches = "directoryBranches";
    public const string TermsAndConditions = "termsAndConditions";
    public const string GuestPermissions = "guestPermissions";
    public const string AppBranding = "appBranding";
    public const string QuickAccessTiles = "quickAccessTiles";
}

public static class ConversationTypes
{
    public const string Direct = "Direct";
    public const string Groups = "Groups";
    public const string Announcements = "Announcements";
}
