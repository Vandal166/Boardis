using Application.Abstractions.CQRS;
using Application.Features.BoardLists.EventHandlers;
using Domain.Board.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardCreatedEventHandler : EventHandlerBase<BoardCreatedEvent>
{
    private readonly IDistributedCache _cache;
    
    public BoardCreatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }
    public override async Task Handle(BoardCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardCreatedEvent handled for BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"boards_{@event.OwnerId}", ct);
    }
}