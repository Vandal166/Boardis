using Application.Abstractions.CQRS;
using Application.Features.BoardLists.EventHandlers;
using Domain.Board.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardCreatedEventHandler : EventHandlerBase<BoardCreatedEvent>
{
    private readonly IDistributedCache _cache;
    
    public BoardCreatedEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }
    public override async Task Handle(BoardCreatedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardCreatedEvent handled for BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");
        
        await _cache.RemoveAsync($"boards_{@event.OwnerId}", ct);
        
        
        //TODO:
        // 1. Send some notification to owner that board is created
        // 2. Add default lists to the board (e.g. To Do, In Progress, Done)
        // 3. Notifications/Communication: Send emails, push notifications, or UI updates (e.g., notify the owner: "Your board is ready").
        // 4. Update Read Models (CQRS): Denormalize data for queries (e.g., update a "UserBoardsView" projection with the new board).
        // 5. Trigger Integrations: Call external services/APIs (e.g., sync to a third-party tool like Slack).
        // 6. Auditing/Logging: Record events for compliance (e.g., log "Board created by user X").
        // 7. Cache Management: Invalidate or warm caches (e.g., evict board-related keys).
        // 8. Cross-Aggregate/Bounded Context Actions: Raise other events or commands (e.g., in another context, create a related "AuditTrail" entity).
        // 9. Analytics/Metrics: Track usage (e.g., increment "boards_created" counter in Prometheus).
        // 10. Event Sourcing Replays: If using event sourcing, handlers rebuild state from events.
        await Task.CompletedTask;
    }
}