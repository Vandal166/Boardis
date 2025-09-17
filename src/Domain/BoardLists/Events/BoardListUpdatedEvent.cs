using Domain.Common;

namespace Domain.BoardLists.Events;

public sealed record BoardListUpdatedEvent(Guid BoardId, Guid BoardListId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}