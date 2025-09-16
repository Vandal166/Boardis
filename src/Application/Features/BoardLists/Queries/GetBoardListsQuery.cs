using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;

namespace Application.Features.BoardLists.Queries;

public sealed record GetBoardListsQuery : IQuery<List<BoardListResponse>>, ICacheableQuery
{
    public required Guid BoardId { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    
    public string CacheKey => $"lists_{BoardId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration { get; } = TimeSpan.FromMinutes(2);
}