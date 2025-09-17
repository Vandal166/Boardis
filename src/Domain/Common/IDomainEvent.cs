namespace Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; } // Unique identifier for the event instance, useful for tracking and logging

    DateTime OccurredOn { get; }
}