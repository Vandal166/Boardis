using System.Data;

namespace Application.Contracts.Persistence;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken ct = default);
}