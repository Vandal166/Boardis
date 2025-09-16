using Application.Abstractions.CQRS;
using Application.DTOs.BoardMembers;

namespace Application.Features.BoardMembers.Queries;

public sealed record GetBoardMembersQuery : IQuery<List<BoardMemberResponse>>, ICacheableQuery
{
    public Guid BoardId { get; init; }
    public Guid RequestingUserId { get; init; }
    
    public string CacheKey => $"board_members_{BoardId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(5);
    public TimeSpan? SlidingExpiration { get; } = TimeSpan.FromMinutes(2);
}