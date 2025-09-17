using Domain.Common;

namespace Domain.MemberPermissions.Events;

public sealed record MemberPermissionRemovedEvent(Guid BoardId, Guid MemberId, string Permission, Guid RemovedBy) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}