namespace Application.DTOs.BoardMembers;

public sealed class BoardMemberResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}