using Application.Contracts;
using Infrastructure.Data;

namespace Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardisDbContext _db;
    public UnitOfWork(BoardisDbContext db) => _db = db;
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}