namespace Domain.Entities;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events occurred.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        this._domainEvents.Add(domainEvent);
    }
}


public interface IDomainEvent
{
    Guid Id { get; } // Unique identifier for the event instance, useful for tracking and logging

    DateTime OccurredOn { get; }
}

public interface IAggregateRoot;    
