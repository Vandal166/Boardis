using Domain.Common;

namespace Domain.ListCards.Events;

public sealed record ListCardUpdatedEvent(Guid BoardId, Guid BoardListId, Guid ListCardId, Guid UpdatedBy) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}