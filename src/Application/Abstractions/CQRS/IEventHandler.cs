using Domain.Common;

namespace Application.Abstractions.CQRS;

public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IDomainEvent
{
     Task Handle(TEvent @event, CancellationToken ct = default);
}

public interface IEventHandler
{
    Task Handle(IDomainEvent @event, CancellationToken ct = default);
}