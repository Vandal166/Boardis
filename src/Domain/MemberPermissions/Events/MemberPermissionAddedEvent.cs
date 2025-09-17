using Domain.Common;

namespace Domain.MemberPermissions.Events;

public sealed record MemberPermissionAddedEvent(Guid BoardId, Guid MemberId, string Permission, Guid AddedBy) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}