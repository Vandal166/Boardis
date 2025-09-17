using Domain.Common;

namespace Domain.ListCards.Events;

public sealed record ListCardDeletedEvent(Guid BoardId, Guid BoardListId, Guid ListCardId, Guid DeletedBy) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}