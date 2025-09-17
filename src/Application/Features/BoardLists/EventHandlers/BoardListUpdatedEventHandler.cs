using Application.Abstractions.CQRS;

using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListUpdatedEventHandler : EventHandlerBase<BoardListUpdatedEvent>
{
    private readonly IDistributedCache _cache;

    public BoardListUpdatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(BoardListUpdatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListUpdatedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
    }
}