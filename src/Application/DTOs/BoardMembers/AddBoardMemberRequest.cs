namespace Application.DTOs.BoardMembers;

public sealed class AddBoardMemberRequest
{
    public required Guid UserId { get; init; }
    public required string Role { get; init; }
}