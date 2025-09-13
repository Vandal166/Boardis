namespace Domain.Constants;

public static class Roles
{
    public static readonly Guid OwnerId = new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef");
    public const string OwnerKey = "Owner";
    public static readonly Guid MemberId = new Guid("fedcba98-7654-3210-fedc-ba9876543210");
    public const string MemberKey = "Member";
}