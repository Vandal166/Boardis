using Domain.Entities;

namespace Domain.Contracts;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken ct = default);
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
}