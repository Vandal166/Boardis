using System.Text.Json;
using Application.Contracts.Board;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Persistence.Board;

internal sealed class CachedBoardMemberRepository : IBoardMemberRepository
{
    private readonly IBoardMemberRepository _innerRepository;
    private readonly IDistributedCache _cache;
    public CachedBoardMemberRepository(IBoardMemberRepository innerRepository, IDistributedCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task AddAsync(BoardMember member, CancellationToken ct = default)
    {
        await _innerRepository.AddAsync(member, ct);
        
        // Invalidate cache
        string cacheKey = $"board_members_{member.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        //invalidating the user that was added to the board
        string userBoardsCacheKey = $"boards_{member.UserId}";
        await _cache.RemoveAsync(userBoardsCacheKey, ct);
    }

    public async Task DeleteAsync(BoardMember member, CancellationToken ct = default)
    {
        await _innerRepository.DeleteAsync(member, ct);
        
        // Invalidate cache
        string cacheKey = $"board_members_{member.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        //invalidating the user that was removed from the board
        string userBoardsCacheKey = $"boards_{member.UserId}";
        await _cache.RemoveAsync(userBoardsCacheKey, ct);
    }

    public async Task UpdateAsync(BoardMember member, CancellationToken ct = default)
    {
        await _innerRepository.UpdateAsync(member, ct);
        
        // Invalidate cache
        string cacheKey = $"board_members_{member.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        //invalidating the user that was updated in the board
        string userBoardsCacheKey = $"boards_{member.UserId}";
        await _cache.RemoveAsync(userBoardsCacheKey, ct);
    }

    public async Task<BoardMember?> GetByIdAsync(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        return await _innerRepository.GetByIdAsync(boardId, userId, ct);
    }

    public async Task<List<BoardMember>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default)
    {
        string cacheKey = $"board_members_{boardId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<BoardMember>>(cachedJson);
            return cachedResponses;
        }
        
        var members = await _innerRepository.GetByBoardIdAsync(boardId, ct);
        
        if (members is not null)
        {
            var json = JsonSerializer.Serialize(members);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, json, options, ct);
        }
        
        return members;
    }
}