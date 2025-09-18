using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Application.Contracts.Keycloak;
using Domain.BoardMembers.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.EventHandlers;

internal sealed class BoardMemberRemovedEventHandler : EventHandlerBase<BoardMemberRemovedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly INotificationNotifier _notificationNotifier;
    private readonly IKeycloakUserService _keycloakUserService;
    public BoardMemberRemovedEventHandler(IDistributedCache cache, INotificationNotifier notificationNotifier, IKeycloakUserService keycloakUserService)
    {
        _cache = cache;
        _notificationNotifier = notificationNotifier;
        _keycloakUserService = keycloakUserService;
    }

    public override async Task Handle(BoardMemberRemovedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardMemberRemovedEvent handled for BoardId: {@event.BoardId}, RemovedUser Id: {@event.RemovedUserId} which occurred on {@event.OccurredOn}");
        
        var byUser = await _keycloakUserService.GetUserByIdAsync(@event.ByUserId, ct);
        if (byUser.IsSuccess)
        {
            Console.WriteLine($"Sending notification to user {@event.RemovedUserId} about being removed from board {@event.BoardId} by {byUser.Value.Username}");
            await _notificationNotifier.NotifyBoardMemberRemovedAsync(@event.BoardId, @event.RemovedUserId, byUser.Value.Username, ct);
        }
        
        await _cache.RemoveAsync($"board_members_{@event.BoardId}", ct); // invalidating board's members cache
        await _cache.RemoveAsync($"boards_{@event.RemovedUserId}", ct); // invalidating the removed user boards cache
    }
}