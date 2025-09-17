using Domain.Common;

namespace Domain.Board.Events;

public sealed record BoardCreatedEvent(Guid BoardId, Guid OwnerId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}