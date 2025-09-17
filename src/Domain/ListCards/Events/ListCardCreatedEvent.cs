using Domain.Common;

namespace Domain.ListCards.Events;

public sealed record ListCardCreatedEvent(Guid BoardId, Guid BoardListId, Guid ListCardId, Guid CreatedBy) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}