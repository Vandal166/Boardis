using Application.Abstractions.CQRS;
using Application.DTOs.BoardMembers;

namespace Application.Features.BoardMembers.Queries;

public sealed record GetBoardMembersQuery : IQuery<List<BoardMemberResponse>>
{
    public Guid BoardId { get; init; }
    public Guid RequestingUserId { get; init; }
}