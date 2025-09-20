using Domain.Common;

namespace Domain.BoardMembers.Events;

public sealed record BoardMemberLeftEvent(Guid BoardId, Guid LeftUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}