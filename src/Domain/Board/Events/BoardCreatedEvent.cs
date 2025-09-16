using Domain.Entities;

namespace Domain.Board.Events;

public sealed record BoardCreatedEvent(Guid BoardId, Guid OwnerId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BoardDeletedEvent(Guid BoardId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BoardUpdatedEvent(Guid BoardId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BoardMemberAddedEvent(Guid BoardId, Guid AddedUserId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BoardMemberRemovedEvent(Guid BoardId, Guid RemovedUserId, Guid ByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

