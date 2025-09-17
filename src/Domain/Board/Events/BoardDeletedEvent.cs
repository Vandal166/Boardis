using Domain.Common;

namespace Domain.Board.Events;

public sealed record BoardDeletedEvent(Guid BoardId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}