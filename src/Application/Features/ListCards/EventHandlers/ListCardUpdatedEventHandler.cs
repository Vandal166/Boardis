using Application.Abstractions.CQRS;
using Domain.ListCards.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.EventHandlers;

internal sealed class ListCardUpdatedEventHandler : EventHandlerBase<ListCardUpdatedEvent>
{
    private readonly IDistributedCache _cache;

    public ListCardUpdatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(ListCardUpdatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"ListCardUpdatedEvent handled for ListCardId: {@event.ListCardId} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct);
    }
}