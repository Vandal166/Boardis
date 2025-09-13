using Domain.Entities;

namespace Domain.Contracts;

public interface IBoardMemberPermissionRepository
{
    Task AddAsync(MemberPermission permission, CancellationToken ct = default);
    Task DeleteAsync(MemberPermission permission, CancellationToken ct = default);
    
    Task<List<MemberPermission>?> GetByIdAsync(Guid boardId, Guid memberId, CancellationToken ct = default);
}