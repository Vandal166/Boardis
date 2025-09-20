using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.BoardLists.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListAddedEventHandler : EventHandlerBase<BoardListAddedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IBoardListNotifier _boardListNotifier;
    public BoardListAddedEventHandler(IDistributedCache cache, IBoardListNotifier boardListNotifier)
    {
        _cache = cache;
        _boardListNotifier = boardListNotifier;
    }

    public override async Task Handle(BoardListAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListAddedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        
        await _boardListNotifier.NotifyBoardListCreatedAsync(@event.BoardId, @event.BoardListId, ct);
        
        await _cache.RemoveAsync($"lists_{@event.BoardId}", ct);
    }
}