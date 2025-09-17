using Application.Abstractions.CQRS;
using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListAddedEventHandler : EventHandlerBase<BoardListAddedEvent>
{
    private readonly IDistributedCache _cache;

    public BoardListAddedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(BoardListAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListAddedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
    }
}