using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.ListCards.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.EventHandlers;

internal sealed class ListCardCreatedEventHandler : EventHandlerBase<ListCardCreatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IListCardNotifier _listCardNotifier;
    public ListCardCreatedEventHandler(IDistributedCache cache, IListCardNotifier listCardNotifier)
    {
        _cache = cache;
        _listCardNotifier = listCardNotifier;
    }

    public override async Task Handle(ListCardCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"ListCardCreatedEvent handled for ListCardId: {@event.ListCardId} which occurred on {@event.OccurredOn}");

        await _listCardNotifier.NotifyListCardCreatedAsync(@event.BoardId, @event.BoardListId, @event.ListCardId, ct);
        
        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct);
    }
}