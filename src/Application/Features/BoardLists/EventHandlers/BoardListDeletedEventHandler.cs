using Application.Abstractions.CQRS;

using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListDeletedEventHandler : EventHandlerBase<BoardListDeletedEvent>
{
    private readonly IDistributedCache _cache;

    public BoardListDeletedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(BoardListDeletedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListDeletedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct); //invalidating the cards cache for the deleted list
    }
}