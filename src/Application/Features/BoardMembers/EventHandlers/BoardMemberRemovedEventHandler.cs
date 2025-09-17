using Application.Abstractions.CQRS;
using Domain.BoardMembers.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.EventHandlers;

internal sealed class BoardMemberRemovedEventHandler : EventHandlerBase<BoardMemberRemovedEvent>
{
    private readonly IDistributedCache _cache;

    public BoardMemberRemovedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(BoardMemberRemovedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardMemberRemovedEvent handled for BoardId: {@event.BoardId}, RemovedUser Id: {@event.RemovedUserId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"board_members_{@event.BoardId}", ct); // invalidating board's members cache
        await _cache.RemoveAsync($"boards_{@event.RemovedUserId}", ct); // invalidating the removed user boards cache
    }
}