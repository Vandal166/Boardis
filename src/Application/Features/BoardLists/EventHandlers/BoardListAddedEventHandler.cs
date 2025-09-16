using Domain.BoardList.Events;

namespace Application.Features.BoardLists.EventHandlers;

internal sealed class BoardListAddedEventHandler : EventHandlerBase<BoardListAddedEvent>
{
    public override async Task Handle(BoardListAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardListAddedEvent handled for BoardListId: {@event.BoardListId} in BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        await Task.CompletedTask;
    }
}