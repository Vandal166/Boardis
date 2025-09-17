using Application.Abstractions.CQRS;
using Domain.BoardMembers.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.EventHandlers;

internal sealed class BoardMemberAddedEventHandler : EventHandlerBase<BoardMemberAddedEvent>
{
    private readonly IDistributedCache _cache;

    public BoardMemberAddedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(BoardMemberAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardMemberAddedEvent handled for BoardId: {@event.BoardId}, AddedUserId: {@event.AddedUserId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"board_members_{@event.BoardId}", ct); // invalidating board's members cache
        await _cache.RemoveAsync($"boards_{@event.AddedUserId}", ct); // invalidating user's boards cache
    }
}