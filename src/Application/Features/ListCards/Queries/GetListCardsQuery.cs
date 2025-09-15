using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;

namespace Application.Features.ListCards.Queries;

public sealed record GetListCardsQuery : IQuery<List<ListCardResponse>>, ICacheableQuery
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public string CacheKey => $"cards_{BoardListId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(5);
    public TimeSpan? SlidingExpiration { get; } = TimeSpan.FromMinutes(2);
}