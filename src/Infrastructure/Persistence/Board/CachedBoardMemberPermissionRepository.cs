using System.Text.Json;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Persistence.Board;

internal sealed class CachedBoardMemberPermissionRepository : IBoardMemberPermissionRepository
{
    private readonly IBoardMemberPermissionRepository _innerRepository;
    private readonly IDistributedCache _cache;

    public CachedBoardMemberPermissionRepository(IBoardMemberPermissionRepository innerRepository, IDistributedCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task AddAsync(MemberPermission permission, CancellationToken ct = default)
    {
        await _innerRepository.AddAsync(permission, ct);

        // Invalidate cache
        string cacheKey = $"board_member_permissions_{permission.BoardId}_{permission.BoardMemberId}";
        await _cache.RemoveAsync(cacheKey, ct);

        // Invalidate board permissions cache
        string boardCacheKey = $"board_member_permissions_{permission.BoardId}";
        await _cache.RemoveAsync(boardCacheKey, ct);
    }

    public async Task DeleteAsync(MemberPermission permission, CancellationToken ct = default)
    {
        await _innerRepository.DeleteAsync(permission, ct);

        // Invalidate cache
        string cacheKey = $"board_member_permissions_{permission.BoardId}_{permission.BoardMemberId}";
        await _cache.RemoveAsync(cacheKey, ct);

        // Invalidate board permissions cache
        string boardCacheKey = $"board_member_permissions_{permission.BoardId}";
        await _cache.RemoveAsync(boardCacheKey, ct);
    }

    public async Task<List<MemberPermission>?> GetByIdAsync(Guid boardId, Guid memberId, CancellationToken ct = default)
    {
        string cacheKey = $"board_member_permissions_{boardId}_{memberId}";
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<List<MemberPermission>>(cachedData);
        }

        var permissions = await _innerRepository.GetByIdAsync(boardId, memberId, ct);

        if (permissions is not null)
        {
            var serializedData = JsonSerializer.Serialize(permissions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedData, options, ct);
        }

        return permissions;
    }
}