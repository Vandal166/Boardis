using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Contracts;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task ExecuteSqlRawAsync(string sql, CancellationToken ct = default);
}