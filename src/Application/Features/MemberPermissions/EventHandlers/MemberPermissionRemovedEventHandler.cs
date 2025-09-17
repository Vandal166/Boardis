using Application.Abstractions.CQRS;
using Domain.MemberPermissions.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.MemberPermissions.EventHandlers;

internal sealed class MemberPermissionRemovedEventHandler : EventHandlerBase<MemberPermissionRemovedEvent>
{
    private readonly IDistributedCache _cache;

    public MemberPermissionRemovedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(MemberPermissionRemovedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"MemberPermissionRemovedEvent handled for BoardId: {@event.BoardId}, MemberId: {@event.MemberId}, Permission: {@event.Permission} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"board_member_permissions_{@event.BoardId}_{@event.MemberId}", ct);
    }
}