using Domain.Common;

namespace Application.Abstractions.CQRS;

public interface IEventPublisher
{
    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}