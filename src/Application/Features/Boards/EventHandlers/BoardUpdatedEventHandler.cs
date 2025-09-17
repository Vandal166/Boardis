using Application.Abstractions.CQRS;
using Domain.Board.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardUpdatedEventHandler : EventHandlerBase<BoardUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    
    public BoardUpdatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }
    public override async Task Handle(BoardUpdatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardUpdatedEvent handled for BoardId: {@event.BoardId} by user: {@event.ByUserId} which occurred on {@event.OccurredOn}");
        
        // Invalidate cache
        await _cache.RemoveAsync($"boards_{@event.ByUserId}", ct);
        
        // Invalidating the board members cache otherwise they won't see the updated board details from their boards list
        foreach (var memberId in @event.MemberIds)
        {
            await _cache.RemoveAsync($"boards_{memberId}", ct);
        }
    }
}