using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListDeletedEventHandler : EventHandlerBase<BoardListDeletedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IBoardListNotifier _boardListNotifier;
    public BoardListDeletedEventHandler(IDistributedCache cache, IBoardListNotifier boardListNotifier)
    {
        _cache = cache;
        _boardListNotifier = boardListNotifier;
    }

    public override async Task Handle(BoardListDeletedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListDeletedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        await _boardListNotifier.NotifyBoardListDeletedAsync(@event.BoardId, @event.BoardListId, ct);
        
        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct); //invalidating the cards cache for the deleted list
    }
}