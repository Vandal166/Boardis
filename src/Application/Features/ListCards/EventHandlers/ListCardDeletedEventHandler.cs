using Application.Abstractions.CQRS;
using Domain.ListCards.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.EventHandlers;

internal sealed class ListCardDeletedEventHandler : EventHandlerBase<ListCardDeletedEvent>
{
    private readonly IDistributedCache _cache;

    public ListCardDeletedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public override async Task Handle(ListCardDeletedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"ListCardDeletedEvent handled for ListCardId: {@event.ListCardId} which occurred on {@event.OccurredOn}");

        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct);
    }
}