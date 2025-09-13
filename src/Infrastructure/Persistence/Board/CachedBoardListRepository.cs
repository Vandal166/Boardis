using System.Text.Json;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Persistence.Board;

internal sealed class CachedBoardListRepository : IBoardListRepository
{
    private readonly IBoardListRepository _innerRepository;
    private readonly IDistributedCache _cache;
    public CachedBoardListRepository(IBoardListRepository innerRepository, IDistributedCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task AddAsync(BoardList boardList, CancellationToken ct = default)
    {
        await _innerRepository.AddAsync(boardList, ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{boardList.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    public async Task DeleteAsync(BoardList boardList, CancellationToken ct = default)
    {
        await _innerRepository.DeleteAsync(boardList, ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{boardList.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    public async Task UpdateAsync(BoardList boardList, CancellationToken ct = default)
    {
        await _innerRepository.UpdateAsync(boardList, ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{boardList.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    public async Task<BoardList?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _innerRepository.GetByIdAsync(id, ct);
    }

    public async Task<List<BoardList>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default)
    {
        string cacheKey = $"lists_{boardId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<BoardList>>(cachedJson);
           
            return cachedResponses;
        }
        
        var boardLists = await _innerRepository.GetByBoardIdAsync(boardId, ct);
        
        if (boardLists is not null)
        {
            var json = JsonSerializer.Serialize(boardLists);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, json, cacheOptions, ct);
        }
        
        return boardLists;
    }

    public async Task<BoardList?> GetByBoardIdAndPositionAsync(Guid boardId, int position, CancellationToken ct = default)
    {
        return await _innerRepository.GetByBoardIdAndPositionAsync(boardId, position, ct);
    }
}