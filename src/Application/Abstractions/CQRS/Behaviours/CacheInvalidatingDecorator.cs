using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Abstractions.CQRS.Behaviours;

/// <summary>
/// Invalidates cache entries after a command is executed if it implements ICacheInvalidatingCommand.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
internal sealed class CacheInvalidatingDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
    private readonly IDistributedCache _cache;

    public CacheInvalidatingDecorator(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IDistributedCache cache)
    {
        _innerHandler = innerHandler;
        _cache = cache;
    }

    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct = default)
    {
        var response = await _innerHandler.Handle(command, ct);

        if (response.IsSuccess && command is ICacheInvalidatingCommand invalidating)
        {
            foreach (var key in invalidating.CacheKeysToInvalidate)
            {
                await _cache.RemoveAsync(key, ct);
            }
        }

        return response;
    }
}

internal sealed class CacheInvalidatingDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _innerHandler;
    private readonly IDistributedCache _cache;

    public CacheInvalidatingDecorator(
        ICommandHandler<TCommand> innerHandler,
        IDistributedCache cache)
    {
        _innerHandler = innerHandler;
        _cache = cache;
    }

    public async Task<Result> Handle(TCommand command, CancellationToken ct = default)
    {
        var response = await _innerHandler.Handle(command, ct);

        if (response.IsSuccess && command is ICacheInvalidatingCommand invalidating)
        {
            foreach (var key in invalidating.CacheKeysToInvalidate)
            {
                await _cache.RemoveAsync(key, ct);
            }
        }

        return response;
    }
}