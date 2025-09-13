using System.Text.Json;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Persistence.Board;

// An decorator for IListCardRepository that adds distributed caching
internal sealed class CachedListCardRepository : IListCardRepository
{
    private readonly IListCardRepository _innerRepository;
    private readonly IDistributedCache _cache;

    public CachedListCardRepository(IListCardRepository innerRepository, IDistributedCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task AddAsync(ListCard listCard, CancellationToken ct = default)
    {
        await _innerRepository.AddAsync(listCard, ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{listCard.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    public async Task DeleteAsync(ListCard listCard, CancellationToken ct = default)
    {
        await _innerRepository.DeleteAsync(listCard, ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{listCard.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    public async Task UpdateAsync(ListCard listCard, CancellationToken ct = default)
    {
        await _innerRepository.UpdateAsync(listCard, ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{listCard.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }
    
    public async Task<ListCard?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _innerRepository.GetByIdAsync(id, ct);
    }
    
    public async Task<List<ListCard>?> GetByBoardListIdAsync(Guid boardListId, CancellationToken ct = default)
    {
        string cacheKey = $"cards_{boardListId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<ListCard>>(cachedJson);
            return cachedResponses;
        }
        
        var listCards = await _innerRepository.GetByBoardListIdAsync(boardListId, ct);
        
        if (listCards is not null) // only cache if there are cards
        {
            // Cache the result with 5-minute expiration
            string json = JsonSerializer.Serialize(listCards);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2) // resetting expiration if accessed within 2 minutes
            }, ct);
        }
        return listCards;
    }
}