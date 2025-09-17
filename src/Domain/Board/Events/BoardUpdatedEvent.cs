using Domain.Common;

namespace Domain.Board.Events;

public sealed record BoardUpdatedEvent(Guid BoardId, Guid ByUserId, List<Guid> MemberIds) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}