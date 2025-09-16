namespace Application.Contracts.Board;

using Board = Domain.Entities.Board;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken ct = default);
    Task DeleteAsync(Board board, CancellationToken ct = default);
    
    
    Task<Board?> GetWithLists(Guid boardId, CancellationToken cancellationToken = default);
    Task<Board?> GetWithCards(Guid boardId, CancellationToken cancellationToken = default);
    Task<Board?> GetWithMembers(Guid boardId, CancellationToken cancellationToken = default);
   
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
}