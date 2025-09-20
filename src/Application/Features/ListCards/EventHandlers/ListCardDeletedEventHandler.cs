using Application.Abstractions.CQRS;
using Application.Contracts.Communication;
using Domain.ListCards.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.EventHandlers;

internal sealed class ListCardDeletedEventHandler : EventHandlerBase<ListCardDeletedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IListCardNotifier _listCardNotifier;
    public ListCardDeletedEventHandler(IDistributedCache cache, IListCardNotifier listCardNotifier)
    {
        _cache = cache;
        _listCardNotifier = listCardNotifier;
    }

    public override async Task Handle(ListCardDeletedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"ListCardDeletedEvent handled for ListCardId: {@event.ListCardId} which occurred on {@event.OccurredOn}");

        await _listCardNotifier.NotifyListCardDeletedAsync(@event.BoardId, @event.BoardListId, @event.ListCardId, ct);
        
        await _cache.RemoveAsync($"cards_{@event.BoardListId}", ct);
    }
}