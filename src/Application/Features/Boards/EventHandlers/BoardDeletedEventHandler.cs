using Application.Abstractions.CQRS;
using Domain.Board.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardDeletedEventHandler : EventHandlerBase<BoardDeletedEvent>
{
    private readonly IDistributedCache _cache;
    
    public BoardDeletedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }
    public override async Task Handle(BoardDeletedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardDeletedEvent handled for BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"boards_{@event.ByUserId}", ct);
        
        //TODO clean cache of all members of the board just like in update
    }
}