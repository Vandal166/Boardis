using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;
// ReSharper disable once SuggestVarOrType_BuiltInTypes

namespace Application.Abstractions.CQRS.Behaviours;

/// <summary>
/// Caches the result of a query if it implements ICacheableQuery
/// and retrieves it from the cache if available.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
internal sealed class CachingDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _innerHandler;
    private readonly IDistributedCache _cache;

    public CachingDecorator(IQueryHandler<TQuery, TResponse> innerHandler, IDistributedCache cache)
    {
        _innerHandler = innerHandler;
        _cache = cache;
    }

    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct = default)
    {
        if (query is not ICacheableQuery cacheable || cacheable.BypassCache)
        {
            return await _innerHandler.Handle(query, ct);
        }
        
        byte[]? cachedBytes = await _cache.GetAsync(cacheable.CacheKey, ct);
        if (cachedBytes is not  null) // if found in cache
        {
            var cachedData = JsonSerializer.Deserialize<TResponse>(cachedBytes)!;
            
            return Result.Ok(cachedData);
        }

        var response = await _innerHandler.Handle(query, ct);

        if (response.IsSuccess)  // if the response is successful, cache it
        {
            byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(response.Value);
            var options = new DistributedCacheEntryOptions();
            
            if (cacheable.SlidingExpiration.HasValue) 
                options.SlidingExpiration = cacheable.SlidingExpiration.Value;
            
            if (cacheable.AbsoluteExpiration.HasValue) 
                options.AbsoluteExpirationRelativeToNow = cacheable.AbsoluteExpiration.Value;

            await _cache.SetAsync(cacheable.CacheKey, serialized, options, ct);
        }

        return response;
    }
}