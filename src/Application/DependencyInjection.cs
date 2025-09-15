using Application.Abstractions.CQRS;
using Application.Abstractions.CQRS.Behaviours;
using Application.Contracts.User;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<ICurrentUser, CurrentUser>();

        // Registering Command Handlers and Query Handlers using Scrutor and adding them as scoped services
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));

        services.Decorate(typeof(IQueryHandler<,>), typeof(CachingDecorator<,>)); // Caching decorator for query handlers that have an Query implementing ICacheableQuery
        services.Decorate(typeof(ICommandHandler<,>), typeof(CacheInvalidatingDecorator<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(CacheInvalidatingDecorator<>));
        return services;
    }
}