using Domain.Entities;

namespace Application.Abstractions.CQRS.Behaviours;

public sealed class DomainEventDispatcher
{
    private readonly IEnumerable<IEventHandler> _handlers;

    public DomainEventDispatcher(IEnumerable<IEventHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }
}