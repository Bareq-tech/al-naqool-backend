using Microsoft.AspNetCore.Identity;

namespace BareqAlNaqool.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public string DisplayRole { get; set; } = "Family Member";
    public string MemberId { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public int ChildrenCount { get; set; }
    public bool IsGuest { get; set; }
    public string AccountStatus { get; set; } = "Active";
    public string? RegistrationRelation { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
