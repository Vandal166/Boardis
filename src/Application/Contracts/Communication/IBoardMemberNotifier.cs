namespace Application.Contracts.Communication;

public interface IBoardMemberNotifier
{
    Task NotifyBoardMemberAddedAsync(Guid boardId, Guid userId, CancellationToken ct = default);
    
    Task NotifyBoardMemberRemovedAsync(Guid boardId, Guid userId, CancellationToken ct = default);

    Task NotifyBoardMemberLeftAsync(Guid boardId, Guid userId, CancellationToken ct = default);
}