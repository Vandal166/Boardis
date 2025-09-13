namespace Application.DTOs.MemberPermissions;

public sealed class BoardMemberPermissionsResponse
{
    public Guid UserId { get; set; }
    public List<string> Permissions { get; set; } = null!;
}