using Application.Abstractions.CQRS;
using Domain.ListCards.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.EventHandlers;

internal sealed class ListCardCreatedEventHandler : EventHandlerBase<ListCardCreatedEvent>
{
    private readonly IDistributedCache _cache;

    public ListCardCreatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(ListCardCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"ListCardCreatedEvent handled for ListCardId: {@event.ListCardId} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct);
    }
}