using Domain.Entities;

namespace Domain.Contracts;

public interface IBoardListRepository
{
    Task AddAsync(BoardList boardList, CancellationToken ct = default);
    Task DeleteAsync(BoardList boardList, CancellationToken ct = default);
    
    Task<BoardList?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BoardList>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default);
}