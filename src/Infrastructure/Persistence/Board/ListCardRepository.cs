using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Board;

internal sealed class ListCardRepository : IListCardRepository
{
    private readonly BoardisDbContext _dbContext;

    public ListCardRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ListCard listCard, CancellationToken ct = default)
    {
        await _dbContext.ListCards.AddAsync(listCard, ct);
    }

    public Task DeleteAsync(ListCard listCard, CancellationToken ct = default)
    {
        _dbContext.ListCards.Remove(listCard);
        return Task.CompletedTask;
    }

    public async Task<ListCard?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.ListCards
            .FirstOrDefaultAsync(lc => lc.Id == id, ct);
    }

    public async Task<List<ListCard>?> GetByBoardListIdAsync(Guid boardListId, CancellationToken ct = default)
    {
        return await _dbContext.ListCards
            .AsNoTracking()
            .Where(lc => lc.BoardListId == boardListId).ToListAsync(ct);
    }
}