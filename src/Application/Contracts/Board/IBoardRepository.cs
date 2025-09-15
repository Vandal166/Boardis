namespace Application.Contracts.Board;

public interface IBoardRepository
{
    Task AddAsync(Domain.Entities.Board board, CancellationToken ct = default);
    Task DeleteAsync(Domain.Entities.Board board, CancellationToken ct = default);
    Task UpdateAsync(Domain.Entities.Board board, CancellationToken ct = default);
    
    Task<Domain.Entities.Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Domain.Entities.Board>?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}