using Application.Abstractions.CQRS;
using Application.DTOs.Boards;

namespace Application.Features.Boards.Queries;

public sealed record GetBoardsQuery : IQuery<List<BoardResponse>>, ICacheableQuery
{
    public required Guid RequestingUserId { get; init; }
    
    public string CacheKey => $"boards_{RequestingUserId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(5);
    public TimeSpan? SlidingExpiration { get; } = TimeSpan.FromMinutes(2);
}