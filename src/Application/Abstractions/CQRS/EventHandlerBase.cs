using Domain.Common;

namespace Application.Abstractions.CQRS;

public abstract class EventHandlerBase<TEvent> : IEventHandler<TEvent> where TEvent : IDomainEvent
{
    public abstract Task Handle(TEvent @event, CancellationToken ct = default);

    Task IEventHandler.Handle(IDomainEvent @event, CancellationToken ct)
    {
        if (@event is TEvent specificEvent)
        {
            return Handle(specificEvent, ct);
        }
        return Task.CompletedTask;
    }
}