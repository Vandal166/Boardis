using Application.Contracts;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardisDbContext _db;
    public UnitOfWork(BoardisDbContext db) => _db = db;
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await _db.Database.BeginTransactionAsync(ct);
    }

    public async Task ExecuteSqlRawAsync(string sql, CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlRawAsync(sql, ct);
    }
}