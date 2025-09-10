using Domain.Contracts;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Board;

using Board = Domain.Entities.Board;

internal sealed class BoardRepository : IBoardRepository
{
    private readonly BoardisDbContext _dbContext;
    public BoardRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task AddAsync(Board board, CancellationToken ct = default)
    {
        await _dbContext.Boards.AddAsync(board, ct);
    }

    public Task DeleteAsync(Board board, CancellationToken ct = default)
    {
        _dbContext.Boards.Remove(board);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Board board, CancellationToken ct = default)
    {
        _dbContext.Boards.Update(board);
        return Task.CompletedTask;
    }

    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<List<Board>?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Boards
            .AsNoTracking()
            .Include(b => b.Members)
            .Where(b => b.Members.Any(m => m.UserId == userId))
            .ToListAsync(ct);
    }
}