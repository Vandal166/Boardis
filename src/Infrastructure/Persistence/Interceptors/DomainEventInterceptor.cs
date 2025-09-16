using Application.Abstractions.CQRS.Behaviours;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventDispatcher _eventDispatcher;

    public DomainEventInterceptor(DomainEventDispatcher eventDispatcher)
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
            await _eventDispatcher.Handle(domainEvent, cancellationToken);
        }
        
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}