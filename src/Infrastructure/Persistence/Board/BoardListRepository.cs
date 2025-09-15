using Application.Contracts.Board;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Board;

internal sealed class BoardListRepository : IBoardListRepository
{
    private readonly BoardisDbContext _dbContext;

    public BoardListRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BoardList boardList, CancellationToken ct = default)
    {
        await _dbContext.BoardLists.AddAsync(boardList, ct);
    }

    public Task DeleteAsync(BoardList boardList, CancellationToken ct = default)
    {
        _dbContext.BoardLists.Remove(boardList);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(BoardList boardList, CancellationToken ct = default)
    {
        _dbContext.BoardLists.Update(boardList);
        await Task.CompletedTask;
    }

    public async Task<BoardList?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.BoardLists
            .FirstOrDefaultAsync(bl => bl.Id == id, ct);
    }

    public async Task<List<BoardList>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default)
    {
        return await _dbContext.BoardLists
            .AsNoTracking()
            .Where(bl => bl.BoardId == boardId).ToListAsync(ct); //TODO pagination
    }
    
    public async Task<BoardList?> GetByBoardIdAndPositionAsync(Guid boardId, int position, CancellationToken ct = default)
    {
        return await _dbContext.BoardLists
            .AsNoTracking()
            .FirstOrDefaultAsync(bl => bl.BoardId == boardId && bl.Position == position, ct);
    }
}