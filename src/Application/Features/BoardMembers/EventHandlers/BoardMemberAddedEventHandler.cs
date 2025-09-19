using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Application.Contracts.Keycloak;
using Domain.BoardMembers.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.EventHandlers;

internal sealed class BoardMemberAddedEventHandler : EventHandlerBase<BoardMemberAddedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly INotificationNotifier _notificationNotifier;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IBoardMemberNotifier _boardMemberNotifier;
    public BoardMemberAddedEventHandler(IDistributedCache cache, INotificationNotifier notificationNotifier, IKeycloakUserService keycloakUserService, IBoardMemberNotifier boardMemberNotifier)
    {
        _cache = cache;
        _notificationNotifier = notificationNotifier;
        _keycloakUserService = keycloakUserService;
        _boardMemberNotifier = boardMemberNotifier;
    }

    public override async Task Handle(BoardMemberAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardMemberAddedEvent handled for BoardId: {@event.BoardId}, AddedUserId: {@event.AddedUserId} which occurred on {@event.OccurredOn}");

        if (@event.AddedUserId != @event.ByUserId) // not notifying ourselves
        {
            var byUser = await _keycloakUserService.GetUserByIdAsync(@event.ByUserId, ct);
            if (byUser.IsSuccess)
            {
                Console.WriteLine($"Sending notification to user {@event.AddedUserId} about being added to board {@event.BoardId} by {byUser.Value.Username}");
                await _notificationNotifier.NotifyBoardInviteAsync(@event.BoardId, @event.AddedUserId, byUser.Value.Username, ct);
            }
        }
        
        await _boardMemberNotifier.NotifyBoardMemberAddedAsync(@event.BoardId, @event.AddedUserId, ct);
        
        
        await _cache.RemoveAsync($"board_members_{@event.BoardId}", ct); // invalidating board's members cache
        await _cache.RemoveAsync($"boards_{@event.AddedUserId}", ct); // invalidating user's boards cache
    }
}