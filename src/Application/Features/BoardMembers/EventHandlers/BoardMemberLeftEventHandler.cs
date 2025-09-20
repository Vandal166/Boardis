using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.BoardMembers.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.EventHandlers;

internal sealed class BoardMemberLeftEventHandler : EventHandlerBase<BoardMemberLeftEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IBoardMemberNotifier _boardMemberNotifier;
    public BoardMemberLeftEventHandler(IDistributedCache cache, IBoardMemberNotifier boardMemberNotifier)
    {
        _cache = cache;
        _boardMemberNotifier = boardMemberNotifier;
    }

    public override async Task Handle(BoardMemberLeftEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardMemberLeftEvent handled for BoardId: {@event.BoardId}, LeftUserId: {@event.LeftUserId} which occurred on {@event.OccurredOn}");
        
        await _boardMemberNotifier.NotifyBoardMemberLeftAsync(@event.BoardId, @event.LeftUserId, ct);
        
        await _cache.RemoveAsync($"board_members_{@event.BoardId}", ct); // invalidating board's members cache
        await _cache.RemoveAsync($"boards_{@event.LeftUserId}", ct); // invalidating the removed user boards cache
    }
}