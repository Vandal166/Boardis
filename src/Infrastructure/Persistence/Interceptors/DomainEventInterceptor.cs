using Application.Abstractions.CQRS;
using Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IEventPublisher _eventDispatcher;

    public DomainEventInterceptor(IEventPublisher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var context = eventData.Context;
        if (context is null) 
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        
        
        var entitiesWithEvents = context.ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();
        
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();
        
        //clear otherwise they will be re-published if the same entity is updated again
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());
        
        //publish
        foreach (var domainEvent in domainEvents)
        {
            await _eventDispatcher.Publish(domainEvent, cancellationToken);
        }
        
        // this Save could still fail, the changes would not be committed but the events would be published already(bad btw) - Outbox pattern could solve this
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}