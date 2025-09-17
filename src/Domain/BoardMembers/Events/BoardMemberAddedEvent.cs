using Domain.Common;

namespace Domain.BoardMembers.Events;

public sealed record BoardMemberAddedEvent(Guid BoardId, Guid AddedUserId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}