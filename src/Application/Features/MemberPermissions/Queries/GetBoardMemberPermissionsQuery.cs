using Application.Abstractions.CQRS;
using Application.DTOs.MemberPermissions;

namespace Application.Features.MemberPermissions.Queries;

public sealed record GetBoardMemberPermissionsQuery : IQuery<BoardMemberPermissionsResponse>, ICacheableQuery
{
    public Guid BoardId { get; init; }
    public Guid MemberId { get; init; }
    public Guid RequestingUserId { get; init; }


    public string CacheKey => $"board_member_permissions_{BoardId}_{MemberId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration { get; } = null;
}