using Domain.Common;

namespace Application.Abstractions.CQRS.Behaviours;

internal sealed class DomainEventDispatcher : IEventPublisher
{
    private readonly IEnumerable<IEventHandler> _handlers;

    public DomainEventDispatcher(IEnumerable<IEventHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }
}