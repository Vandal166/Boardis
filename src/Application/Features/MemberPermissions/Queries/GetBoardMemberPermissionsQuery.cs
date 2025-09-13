using Application.Abstractions.CQRS;
using Application.DTOs.MemberPermissions;

namespace Application.Features.MemberPermissions.Queries;

public sealed record GetBoardMemberPermissionsQuery : IQuery<BoardMemberPermissionsResponse>
{
    public Guid BoardId { get; init; }
    public Guid MemberId { get; init; }
    public Guid RequestingUserId { get; init; }
}