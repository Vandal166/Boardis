using Application.Abstractions.CQRS;
using Domain.MemberPermissions.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.MemberPermissions.EventHandlers;

internal sealed class MemberPermissionAddedEventHandler : EventHandlerBase<MemberPermissionAddedEvent>
{
    private readonly IDistributedCache _cache;

    public MemberPermissionAddedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(MemberPermissionAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"MemberPermissionAddedEvent handled for BoardId: {@event.BoardId}, MemberId: {@event.MemberId}, Permission: {@event.Permission} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"board_member_permissions_{@event.BoardId}_{@event.MemberId}", ct);
    }
}