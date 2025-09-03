using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Board;

internal sealed class BoardMemberRepository : IBoardMemberRepository
{
    private readonly BoardisDbContext _dbContext;
    public BoardMemberRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task AddAsync(BoardMember member, CancellationToken ct = default)
    {
        await _dbContext.BoardMembers.AddAsync(member, ct);
    }

    public async Task DeleteAsync(BoardMember member, CancellationToken ct = default)
    {
        _dbContext.BoardMembers.Remove(member);
        await Task.CompletedTask;
    }

    public async Task<BoardMember?> GetByIdAsync(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.BoardMembers
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId, ct);
    }

    public async Task<List<BoardMember>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default)
    {
        return await _dbContext.BoardMembers
            .AsNoTracking()
            .Where(m => m.BoardId == boardId)
            .ToListAsync(ct);
    }
}