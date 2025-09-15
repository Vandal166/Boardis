using Domain.Entities;

namespace Application.Contracts.Board;

public interface IBoardMemberRepository
{
    Task AddAsync(BoardMember member, CancellationToken ct = default);
    Task DeleteAsync(BoardMember member, CancellationToken ct = default);
    Task UpdateAsync(BoardMember member, CancellationToken ct = default);
    
    Task<BoardMember?> GetByIdAsync(Guid boardId, Guid userId, CancellationToken ct = default);
    Task<List<BoardMember>?> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default);
}