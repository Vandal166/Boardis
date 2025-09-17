using Domain.Common;

namespace Domain.BoardMembers.Events;

public sealed record BoardMemberRemovedEvent(Guid BoardId, Guid RemovedUserId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}