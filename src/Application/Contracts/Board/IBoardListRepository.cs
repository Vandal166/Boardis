using Domain.Entities;

namespace Application.Contracts.Board;

public interface IBoardListRepository
{
    Task AddAsync(BoardList boardList, CancellationToken ct = default);
    Task DeleteAsync(BoardList boardList, CancellationToken ct = default);
    Task UpdateAsync(BoardList boardList, CancellationToken ct = default);
    
    Task<BoardList?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BoardList>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default);
    Task<BoardList?> GetByBoardIdAndPositionAsync(Guid boardId, int position, CancellationToken ct = default);
}