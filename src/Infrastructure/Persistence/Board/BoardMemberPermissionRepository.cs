using Application.Contracts.Board;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Board;

internal sealed class BoardMemberPermissionRepository : IBoardMemberPermissionRepository
{
    private readonly BoardisDbContext _dbContext;
    public BoardMemberPermissionRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(MemberPermission permission, CancellationToken ct = default)
    {
        _dbContext.MemberPermissions.Add(permission);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MemberPermission permission, CancellationToken ct = default)
    {
        _dbContext.MemberPermissions.Remove(permission);
        return Task.CompletedTask;
    }

    public async Task<List<MemberPermission>?> GetByIdAsync(Guid boardId, Guid memberId, CancellationToken ct = default)
    {
        return await _dbContext.MemberPermissions
            .AsNoTracking()
            .Where(mp => mp.BoardId == boardId && mp.BoardMemberId == memberId)
            .ToListAsync(ct);
    }
}