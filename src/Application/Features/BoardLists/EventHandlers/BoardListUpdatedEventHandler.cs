using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListUpdatedEventHandler : EventHandlerBase<BoardListUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IBoardListNotifier _boardListNotifier;
    public BoardListUpdatedEventHandler(IDistributedCache cache, IBoardListNotifier boardListNotifier)
    {
        _cache = cache;
        _boardListNotifier = boardListNotifier;
    }

    public override async Task Handle(BoardListUpdatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListUpdatedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        await _boardListNotifier.NotifyBoardListUpdatedAsync(@event.BoardId, @event.BoardListId, ct);
        
        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
    }
}