using Application.Contracts.Board;
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
    
    public async Task<Board?> GetWithLists(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(b => b.BoardLists)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }
    
    public async Task<Board?> GetWithCards(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(b => b.BoardLists)
            .ThenInclude(l => l.Cards)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }
    
    public async Task<Board?> GetWithMembers(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(b => b.Members)
            .ThenInclude(m => m.Permissions)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }
    
    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Boards
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }
}